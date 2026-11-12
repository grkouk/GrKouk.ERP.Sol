// Phone app for entering supplier payments. Plain JavaScript on purpose: the site is served
// over plain HTTP, so there is no service worker to cache a larger framework download.
(function () {
    'use strict';

    // Resolves to <site root>/api/ from both /m/ and /m/index.html.
    var API = new URL('../api/', location.href).href;
    var HEALTH = new URL('../health', location.href).href;
    var REQUEST_TIMEOUT_MS = 30000;
    var PING_TIMEOUT_MS = 8000;
    var PING_INTERVAL_MS = 30000;
    var MAX_SUPPLIER_ROWS = 60;

    var KEY = {
        access: 'mp.accessToken',
        refresh: 'mp.refreshToken',
        company: 'mp.companyCode',
        cfAccount: 'mp.cfAccountId'
    };

    function $(id) { return document.getElementById(id); }

    // ---------- storage ----------

    var store = {
        get: function (k) {
            try { return localStorage.getItem(k); } catch (e) { return null; }
        },
        set: function (k, v) {
            try {
                if (v === null || v === undefined) localStorage.removeItem(k);
                else localStorage.setItem(k, String(v));
            } catch (e) { /* private mode: the app still works for this session */ }
        }
    };

    // ---------- small helpers ----------

    // The server may answer in camelCase or PascalCase depending on serializer settings.
    function camel(v) {
        if (Array.isArray(v)) return v.map(camel);
        if (v && typeof v === 'object') {
            var o = {};
            Object.keys(v).forEach(function (k) {
                o[k.charAt(0).toLowerCase() + k.slice(1)] = camel(v[k]);
            });
            return o;
        }
        return v;
    }

    function el(tag, className, text) {
        var e = document.createElement(tag);
        if (className) e.className = className;
        if (text !== undefined && text !== null) e.textContent = text;
        return e;
    }

    function pad2(n) { return n < 10 ? '0' + n : String(n); }

    function isoDate(d) {
        return d.getFullYear() + '-' + pad2(d.getMonth() + 1) + '-' + pad2(d.getDate());
    }

    function todayIso() { return isoDate(new Date()); }

    function monthStartIso() {
        var d = new Date();
        return isoDate(new Date(d.getFullYear(), d.getMonth(), 1));
    }

    // "2026-11-12T00:00:00" -> "12/11/2026", by slicing so no time zone shift can occur.
    function displayDate(iso) {
        if (!iso) return '';
        return String(iso).slice(0, 10).split('-').reverse().join('/');
    }

    var moneyFormat = new Intl.NumberFormat('el-GR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    function money(n) { return moneyFormat.format(Number(n) || 0) + ' €'; }

    // Accepts "12,50" or "12.50". Thousands separators are rejected rather than guessed at,
    // because "1.250" is ambiguous between 1250 and 1.25.
    function parseAmount(text) {
        var t = String(text || '').replace(/\s/g, '');
        if (!/^\d+([.,]\d{1,2})?$/.test(t)) return null;
        return Number(t.replace(',', '.'));
    }

    function amountText(n) {
        return (Math.round(Number(n) * 100) / 100).toFixed(2).replace('.', ',');
    }

    // Accent- and case-insensitive Greek matching.
    function fold(s) {
        return String(s || '')
            .normalize('NFD')
            .replace(/[̀-ͯ]/g, '')
            .toLowerCase()
            .replace(/ς/g, 'σ');
    }

    // Same key-position table as the server's KeyboardTransliterator: text typed with the wrong
    // keyboard layout still finds the name ("isaa" finds ΙΣΑΑΚ, "ψοψα" finds COCA).
    var EN_KEYS = 'abcdefghijklmnopqrstuvwxyz';
    var GR_KEYS = 'αβψδεφγηιξκλμνοπ;ρστθωςχυζ';

    function swapKeyboard(text) {
        var s = String(text || '').normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase();
        var out = '';
        for (var i = 0; i < s.length; i++) {
            var ch = s.charAt(i);
            var en = EN_KEYS.indexOf(ch);
            var gr = GR_KEYS.indexOf(ch);
            out += en >= 0 ? GR_KEYS.charAt(en) : gr >= 0 ? EN_KEYS.charAt(gr) : ch;
        }
        return out;
    }

    // crypto.randomUUID needs HTTPS; getRandomValues works on plain HTTP too.
    function newKey() {
        var bytes = new Uint8Array(16);
        if (window.crypto && crypto.getRandomValues) {
            crypto.getRandomValues(bytes);
        } else {
            for (var i = 0; i < bytes.length; i++) bytes[i] = Math.floor(Math.random() * 256);
        }
        return Array.prototype.map.call(bytes, function (b) { return ('0' + b.toString(16)).slice(-2); }).join('');
    }

    var toastTimer = null;

    function toast(message, kind) {
        var t = $('toast');
        t.textContent = message;
        t.className = 'toast' + (kind ? ' ' + kind : '');
        t.hidden = false;
        clearTimeout(toastTimer);
        toastTimer = setTimeout(function () { t.hidden = true; }, kind === 'bad' ? 6000 : 2500);
    }

    function showError(id, message) {
        var e = $(id);
        e.textContent = message || '';
        e.hidden = !message;
    }

    // ---------- API access ----------

    function SessionExpired() { this.message = 'Η σύνδεση έληξε. Συνδεθείτε ξανά.'; }

    function fetchWithTimeout(url, options) {
        var controller = new AbortController();
        var timer = setTimeout(function () { controller.abort(); }, REQUEST_TIMEOUT_MS);
        options = options || {};
        options.signal = controller.signal;
        options.cache = 'no-store';
        return fetch(url, options).then(
            function (r) {
                clearTimeout(timer);
                // Only a success proves it is the ERP. Something else can sit on the same address
                // while the ERP is stopped (macOS AirPlay answers 403 on port 5000).
                if (r.ok) setServer('on');
                return r;
            },
            function () {
                clearTimeout(timer);
                setServer('off');
                throw new Error(NO_SERVER);
            });
    }

    // ---------- server status ----------

    var SERVER_TEXT = {
        on: 'Ο διακομιστής είναι διαθέσιμος',
        warn: 'Ο διακομιστής απαντά, αλλά αναφέρει πρόβλημα',
        off: 'Δεν υπάρχει σύνδεση με τον διακομιστή',
        unknown: 'Έλεγχος σύνδεσης…'
    };

    // Shown as a coloured strip in place of the address while the server cannot be used.
    var SERVER_STRIP = {
        off: 'Εκτός σύνδεσης — δεν αποθηκεύονται πληρωμές',
        warn: 'Πρόβλημα στον διακομιστή — δεν αποθηκεύονται πληρωμές'
    };

    var serverStatus = 'unknown';

    function serverDown() { return serverStatus === 'off' || serverStatus === 'warn'; }

    function setServer(status) {
        serverStatus = status;
        var marks = document.querySelectorAll('.server');
        for (var i = 0; i < marks.length; i++) {
            marks[i].className = 'server' + (status === 'unknown' ? '' : ' ' + status);
            marks[i].title = SERVER_TEXT[status];
            marks[i].setAttribute('aria-label', location.host + ': ' + SERVER_TEXT[status]);
            marks[i].querySelector('.msg').textContent = SERVER_STRIP[status] || '';
        }
        refreshButtons();
    }

    var NO_SERVER = 'Δεν υπάρχει σύνδεση με τον διακομιστή.';
    var checkInFlight = null;

    // /health needs no login and also checks the database, so green means payments can be saved.
    // The body is read too: only the ERP answers "Healthy"/"Unhealthy", so another program
    // listening on the same address counts as "no server". Resolves to 'on', 'warn' or 'off'.
    function checkServer() {
        if (!checkInFlight) {
            var controller = new AbortController();
            var timer = setTimeout(function () { controller.abort(); }, PING_TIMEOUT_MS);
            checkInFlight = fetch(HEALTH, { cache: 'no-store', signal: controller.signal })
                .then(function (r) {
                    return r.text().then(function (body) {
                        if (r.ok && /^(Healthy|Degraded)/.test(body)) return 'on';
                        if (r.status === 503 && /^Unhealthy/.test(body)) return 'warn';
                        return 'off';
                    });
                })
                .catch(function () { return 'off'; })
                .then(function (status) {
                    clearTimeout(timer);
                    checkInFlight = null;
                    setServer(status);
                    return status;
                });
        }
        return checkInFlight;
    }

    function pingServer() {
        if (!document.hidden) checkServer();
    }

    function startServerWatch() {
        var hosts = document.querySelectorAll('.server .host');
        for (var i = 0; i < hosts.length; i++) hosts[i].textContent = location.host;
        // Tapping the line checks again at once, so a wrong "offline" never has to be waited out.
        var marks = document.querySelectorAll('.server');
        for (var j = 0; j < marks.length; j++) marks[j].addEventListener('click', checkServer);
        setServer('unknown');
        pingServer();
        setInterval(pingServer, PING_INTERVAL_MS);
        document.addEventListener('visibilitychange', pingServer);
        window.addEventListener('online', pingServer);
        window.addEventListener('offline', function () { setServer('off'); });
    }

    function tokenExpiresSoon(token) {
        try {
            var payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
            return JSON.parse(atob(payload)).exp * 1000 - Date.now() < 60000;
        } catch (e) {
            return true;
        }
    }

    // The server's refresh tokens are single-use, so concurrent callers must share one refresh.
    var refreshInFlight = null;

    function refreshTokens() {
        if (!refreshInFlight) {
            refreshInFlight = (async function () {
                var access = store.get(KEY.access);
                var refresh = store.get(KEY.refresh);
                if (!access || !refresh) return false;
                var r = await fetchWithTimeout(API + 'auth/refresh', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ token: access, refreshToken: refresh })
                });
                // Not the ERP answering: keep the session instead of signing the phone out.
                if (!r.ok && (await checkServer()) === 'off') throw new Error(NO_SERVER);
                if (!r.ok) return false;
                var data = camel(await r.json());
                if (!data.accessToken || !data.refreshToken) return false;
                store.set(KEY.access, data.accessToken);
                store.set(KEY.refresh, data.refreshToken);
                return true;
            })();
            refreshInFlight.then(clearRefresh, clearRefresh);
        }
        return refreshInFlight;
    }

    function clearRefresh() { refreshInFlight = null; }

    async function errorText(r) {
        var text = '';
        try { text = await r.text(); } catch (e) { /* ignore */ }
        try {
            var body = camel(JSON.parse(text));
            if (body && typeof body === 'object') {
                if (body.error) return String(body.error);
                if (body.title) return String(body.title);
            }
            if (typeof body === 'string') return body;
        } catch (e) { /* not JSON */ }
        if (text && text.length < 300 && text.indexOf('<') < 0) return text;
        return 'Σφάλμα διακομιστή (' + r.status + ').';
    }

    async function api(method, path, body, extraHeaders) {
        var access = store.get(KEY.access);
        if (!access) throw new SessionExpired();
        if (tokenExpiresSoon(access)) {
            if (!(await refreshTokens())) throw new SessionExpired();
            access = store.get(KEY.access);
        }

        function send(token) {
            var headers = { 'Authorization': 'Bearer ' + token, 'Accept': 'application/json' };
            if (body !== undefined && body !== null) headers['Content-Type'] = 'application/json';
            Object.keys(extraHeaders || {}).forEach(function (k) { headers[k] = extraHeaders[k]; });
            return fetchWithTimeout(API + path, {
                method: method,
                headers: headers,
                body: body !== undefined && body !== null ? JSON.stringify(body) : undefined
            });
        }

        var r = await send(access);
        if (r.status === 401) {
            if (!(await refreshTokens())) throw new SessionExpired();
            r = await send(store.get(KEY.access));
            if (r.status === 401) throw new SessionExpired();
        }

        var noPermission = 'Ο χρήστης δεν έχει δικαίωμα για αυτή την ενέργεια (απαιτείται ρόλος Admin).';
        // A refusal may come from another program on the address, not from the ERP.
        if (!r.ok && (await checkServer()) === 'off') throw new Error(NO_SERVER);
        if (r.status === 403) throw new Error(noPermission);
        if (!r.ok) throw new Error(await errorText(r));
        if (r.status === 204) return null;

        var text = await r.text();
        if (!text) return null;
        try {
            return camel(JSON.parse(text));
        } catch (e) {
            // A refused request can be redirected to the site's HTML "access denied" page.
            throw new Error(noPermission);
        }
    }

    function query(params) {
        return Object.keys(params)
            .filter(function (k) { return params[k] !== null && params[k] !== undefined && params[k] !== ''; })
            .map(function (k) { return encodeURIComponent(k) + '=' + encodeURIComponent(params[k]); })
            .join('&');
    }

    // ---------- state ----------

    var state = {
        companies: [],      // { code, name }
        suppliers: [],      // { erpId, name, taxNumber, busCode, folded }
        supplier: null,     // selected { erpId, name }
        editing: null,      // the loaded payment when editing, null for a new one
        saveKey: newKey(),  // one key per new payment; reused on retry so a retry cannot duplicate
        listLoaded: false,
        balanceSeq: 0,
        listSeq: 0,
        dataSeq: 0
    };

    function companyCode() { return $('companySelect').value; }

    // ---------- views ----------

    function showLogin(message) {
        $('mainView').hidden = true;
        $('supplierPicker').hidden = true;
        $('loginView').hidden = false;
        showError('loginError', message || '');
        $('loginPass').value = '';
    }

    function handleError(e, errorId) {
        if (e instanceof SessionExpired) {
            store.set(KEY.access, null);
            store.set(KEY.refresh, null);
            showLogin(e.message);
            return;
        }
        var message = (e && e.message) || 'Άγνωστο σφάλμα.';
        if (errorId) showError(errorId, message);
        else toast(message, 'bad');
    }

    function showTab(name) {
        var isForm = name === 'form';
        $('formTab').hidden = !isForm;
        $('listTab').hidden = isForm;
        $('tabFormBtn').classList.toggle('active', isForm);
        $('tabListBtn').classList.toggle('active', !isForm);
        window.scrollTo(0, 0);
        if (!isForm && !state.listLoaded) loadList();
    }

    // ---------- login ----------

    async function onLogin(ev) {
        ev.preventDefault();
        showError('loginError', '');
        var btn = $('loginBtn');
        btn.disabled = true;
        try {
            var r = await fetchWithTimeout(API + 'auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username: $('loginUser').value.trim(), password: $('loginPass').value })
            });
            if (!r.ok && (await checkServer()) === 'off') throw new Error(NO_SERVER);
            if (r.status === 401) throw new Error('Λάθος όνομα χρήστη ή κωδικός.');
            if (!r.ok) throw new Error(await errorText(r));
            var data = camel(await r.json());
            if (!data.accessToken || !data.refreshToken) throw new Error('Μη αναμενόμενη απάντηση από τον διακομιστή.');
            store.set(KEY.access, data.accessToken);
            store.set(KEY.refresh, data.refreshToken);
            $('loginPass').value = '';
            await enterApp();
        } catch (e) {
            showError('loginError', (e && e.message) || 'Η σύνδεση απέτυχε.');
        } finally {
            btn.disabled = false;
        }
    }

    // Only this phone is signed out. The server's "revoke all" is deliberately not called:
    // the same ERP user may be signed in on the shop computers.
    function onLogout() {
        store.set(KEY.access, null);
        store.set(KEY.refresh, null);
        showLogin('');
    }

    // ---------- start-up data ----------

    async function enterApp() {
        try {
            state.companies = (await api('GET', 'CashierApi/companies')) || [];
        } catch (e) {
            if (e instanceof SessionExpired) { handleError(e); return; }
            showLogin((e && e.message) || 'Η φόρτωση απέτυχε.');
            return;
        }

        var select = $('companySelect');
        select.textContent = '';
        state.companies.forEach(function (c) {
            var option = el('option', null, c.name ? c.code + ' — ' + c.name : c.code);
            option.value = c.code;
            select.appendChild(option);
        });
        var last = store.get(KEY.company);
        if (last && state.companies.some(function (c) { return c.code === last; })) select.value = last;

        $('loginView').hidden = true;
        $('mainView').hidden = false;
        $('listFrom').value = monthStartIso();
        $('listTo').value = todayIso();
        resetForm();
        showTab('form');
        await loadCompanyData();
    }

    async function loadCompanyData() {
        var code = companyCode();
        var seq = ++state.dataSeq;
        state.suppliers = [];
        state.listLoaded = false;
        if (!code) {
            toast('Δεν βρέθηκαν εταιρείες.', 'bad');
            return;
        }
        store.set(KEY.company, code);
        try {
            var results = await Promise.all([
                api('GET', 'CashierApi/GetErpLookupSuppliers?' + query({ companyCode: code })),
                api('GET', 'CashierApi/cfaccounts?' + query({ companyCode: code }))
            ]);
            if (seq !== state.dataSeq) return;

            state.suppliers = (results[0] || []).map(function (s) {
                return {
                    erpId: s.erpId,
                    name: s.name || '',
                    taxNumber: s.taxNumber || '',
                    busCode: s.busCode || '',
                    folded: fold(s.name)
                };
            });

            var select = $('cfAccount');
            var wanted = select.dataset.wanted || store.get(KEY.cfAccount);
            select.textContent = '';
            (results[1] || []).forEach(function (a) {
                var option = el('option', null, a.name);
                option.value = String(a.id);
                select.appendChild(option);
            });
            if (wanted && select.querySelector('option[value="' + String(wanted).replace(/"/g, '') + '"]')) {
                select.value = String(wanted);
            }
            delete select.dataset.wanted;
        } catch (e) {
            handleError(e);
        }
        if (!$('listTab').hidden) loadList();
    }

    function onCompanyChange() {
        resetForm();
        loadCompanyData();
    }

    // ---------- supplier picker ----------

    function openSupplierPicker() {
        $('supplierPicker').hidden = false;
        $('supplierSearch').value = '';
        renderSuppliers();
        $('supplierSearch').focus();
    }

    function closeSupplierPicker() { $('supplierPicker').hidden = true; }

    function renderSuppliers() {
        var box = $('supplierRows');
        box.textContent = '';
        var raw = $('supplierSearch').value.trim();
        // Each word may match as typed or as it would read on the other keyboard layout.
        var terms = raw.split(/\s+/).filter(Boolean).map(function (t) {
            return { typed: fold(t), swapped: fold(swapKeyboard(t)) };
        });
        var rawLower = raw.toLowerCase();

        var matches = state.suppliers.filter(function (s) {
            if (terms.length === 0) return true;
            if (terms.every(function (t) {
                return s.folded.indexOf(t.typed) >= 0 || s.folded.indexOf(t.swapped) >= 0;
            })) return true;
            return (s.taxNumber && s.taxNumber.indexOf(raw) >= 0) ||
                   (s.busCode && s.busCode.toLowerCase().indexOf(rawLower) >= 0);
        });

        if (matches.length === 0) {
            box.appendChild(el('p', 'empty',
                state.suppliers.length === 0 ? 'Δεν υπάρχουν προμηθευτές για αυτή την εταιρεία.' : 'Δεν βρέθηκε προμηθευτής.'));
            return;
        }

        matches.slice(0, MAX_SUPPLIER_ROWS).forEach(function (s) {
            var row = el('button', 'item');
            row.type = 'button';
            row.appendChild(el('div', 'name', s.name));
            if (s.taxNumber) row.appendChild(el('div', 'line2', 'ΑΦΜ ' + s.taxNumber));
            row.addEventListener('click', function () {
                selectSupplier({ erpId: s.erpId, name: s.name });
                closeSupplierPicker();
            });
            box.appendChild(row);
        });

        if (matches.length > MAX_SUPPLIER_ROWS) {
            box.appendChild(el('p', 'empty', 'Εμφανίζονται οι πρώτοι ' + MAX_SUPPLIER_ROWS + '. Γράψτε περισσότερα για να περιορίσετε.'));
        }
    }

    function selectSupplier(supplier) {
        state.supplier = supplier;
        var btn = $('supplierBtn');
        btn.textContent = supplier ? supplier.name : 'Επιλογή προμηθευτή…';
        btn.classList.toggle('placeholder', !supplier);
        loadBalance();
    }

    // Balance pooled over all companies: the supplier holds one account for the whole business.
    async function loadBalance() {
        var line = $('balanceLine');
        var seq = ++state.balanceSeq;
        line.hidden = true;
        line.textContent = '';
        if (!state.supplier || state.companies.length === 0) return;
        try {
            var codes = state.companies.map(function (c) { return c.code; }).join(',');
            var data = await api('GET', 'CashierApi/GetErpSupplierOpenItems?' +
                query({ companyCodes: codes, transactorId: state.supplier.erpId }));
            if (seq !== state.balanceSeq || !data) return;
            line.appendChild(document.createTextNode('Υπόλοιπο: '));
            line.appendChild(el('strong', null, money(data.combinedBalance)));
            line.appendChild(document.createTextNode(' · ληξιπρόθεσμα: ' + money(data.currentTotal)));
            line.hidden = false;
        } catch (e) {
            if (e instanceof SessionExpired) handleError(e);
            // Otherwise the balance is only informative: a failure here must not block a payment.
        }
    }

    // ---------- payment form ----------

    function resetForm() {
        state.editing = null;
        state.saveKey = newKey();
        $('formTitle').textContent = 'Νέα πληρωμή';
        $('payDate').value = todayIso();
        $('payAmount').value = '';
        $('payRef').value = '';
        $('payEtiology').value = '';
        $('amountNote').hidden = true;
        $('editActions').hidden = true;
        $('companySelect').disabled = false;
        disarmDelete();
        showError('formError', '');
        selectSupplier(null);
    }

    var saving = false;

    function setBusy(busy) {
        saving = busy;
        refreshButtons();
    }

    // Save and delete are also held back while the server cannot be reached.
    function refreshButtons() {
        var down = serverDown();
        $('saveBtn').disabled = saving || down;
        $('deleteBtn').disabled = saving || down;
        $('cancelEditBtn').disabled = saving;
        $('saveBtn').textContent = saving ? 'Αποθήκευση…' : down ? 'Εκτός σύνδεσης' : 'Αποθήκευση';
    }

    async function onSave(ev) {
        ev.preventDefault();
        showError('formError', '');

        var amount = parseAmount($('payAmount').value);
        var date = $('payDate').value;
        var cfAccountId = Number($('cfAccount').value);

        if (!state.supplier) return showError('formError', 'Επιλέξτε προμηθευτή.');
        if (!date) return showError('formError', 'Συμπληρώστε ημερομηνία.');
        if (amount === null) return showError('formError', 'Το ποσό δεν είναι έγκυρο. Παράδειγμα: 125,50');
        if (amount <= 0) return showError('formError', 'Το ποσό πρέπει να είναι μεγαλύτερο από μηδέν.');
        if (!cfAccountId) return showError('formError', 'Επιλέξτε λογαριασμό πληρωμής.');

        var editing = state.editing;
        var dto = {
            id: editing ? editing.id : null,
            companyCode: editing ? editing.companyCode : companyCode(),
            transactorId: state.supplier.erpId,
            cfAccountId: cfAccountId,
            transDate: date + 'T00:00:00',
            transRefCode: $('payRef').value.trim() || null,
            amountNet: amount,
            // The phone edits one amount. VAT and discount parts entered elsewhere are kept as they are.
            amountFpa: editing ? editing.amountFpa : 0,
            amountDiscount: editing ? editing.amountDiscount : 0,
            fpaRate: editing ? editing.fpaRate : 0,
            discountRate: editing ? editing.discountRate : 0,
            etiology: $('payEtiology').value.trim() || null,
            timestamp: editing ? editing.timestamp : null
        };

        setBusy(true);
        try {
            if (editing) {
                await api('PUT', 'CashierApi/supplier-payments/' + editing.id, dto);
            } else {
                await api('POST', 'CashierApi/supplier-payments', dto, { 'X-Idempotency-Key': state.saveKey });
            }
            store.set(KEY.cfAccount, cfAccountId);
            toast(editing ? 'Η πληρωμή ενημερώθηκε.' : 'Η πληρωμή αποθηκεύτηκε.', 'ok');
            state.listLoaded = false;
            resetForm();
            if (editing) showTab('list');
        } catch (e) {
            handleError(e, 'formError');
        } finally {
            setBusy(false);
        }
    }

    async function startEdit(row) {
        try {
            var dto = await api('GET', 'CashierApi/supplier-payments/' + row.id);
            if (!dto) throw new Error('Η πληρωμή δεν βρέθηκε.');
            resetForm();
            state.editing = dto;
            $('formTitle').textContent = 'Διόρθωση πληρωμής';
            $('payDate').value = String(dto.transDate).slice(0, 10);
            $('payAmount').value = amountText(dto.amountNet);
            $('payRef').value = dto.transRefCode || '';
            $('payEtiology').value = dto.etiology || '';

            var select = $('cfAccount');
            if (select.querySelector('option[value="' + Number(dto.cfAccountId) + '"]')) {
                select.value = String(dto.cfAccountId);
            }

            var extra = Number(dto.amountFpa) - Number(dto.amountDiscount);
            if (Number(dto.amountFpa) !== 0 || Number(dto.amountDiscount) !== 0) {
                $('amountNote').textContent = 'Η πληρωμή έχει και ΦΠΑ/έκπτωση ' + money(extra) +
                    ' που δεν αλλάζουν από εδώ. Το ποσό παραπάνω είναι το καθαρό.';
                $('amountNote').hidden = false;
            }

            $('editActions').hidden = false;
            // A payment keeps the company it was entered for.
            $('companySelect').disabled = true;
            selectSupplier({ erpId: dto.transactorId, name: row.transactorName || '' });
            showTab('form');
        } catch (e) {
            handleError(e);
        }
    }

    // Delete asks for a second tap instead of a browser confirm box.
    var deleteArmTimer = null;

    function disarmDelete() {
        clearTimeout(deleteArmTimer);
        deleteArmTimer = null;
        $('deleteBtn').textContent = 'Διαγραφή';
    }

    async function onDelete() {
        if (!state.editing) return;
        if (!deleteArmTimer) {
            $('deleteBtn').textContent = 'Πατήστε ξανά για διαγραφή';
            deleteArmTimer = setTimeout(disarmDelete, 4000);
            return;
        }
        disarmDelete();
        setBusy(true);
        try {
            await api('DELETE', 'CashierApi/supplier-payments/' + state.editing.id);
            toast('Η πληρωμή διαγράφηκε.', 'ok');
            state.listLoaded = false;
            resetForm();
            showTab('list');
        } catch (e) {
            handleError(e, 'formError');
        } finally {
            setBusy(false);
        }
    }

    function onCancelEdit() {
        resetForm();
        showTab('list');
    }

    // ---------- payments list ----------

    async function loadList() {
        var seq = ++state.listSeq;
        var box = $('listRows');
        var code = companyCode();
        if (!code) return;
        $('listSummary').textContent = 'Φόρτωση…';
        try {
            var rows = await api('GET', 'CashierApi/GetErpSupplierPayments?' + query({
                companyCode: code,
                searchText: $('listSearch').value.trim(),
                dateFrom: $('listFrom').value,
                dateTo: $('listTo').value
            }));
            if (seq !== state.listSeq) return;
            rows = rows || [];
            state.listLoaded = true;

            // Newest first.
            rows.sort(function (a, b) {
                var d = String(b.transDate).localeCompare(String(a.transDate));
                return d !== 0 ? d : b.id - a.id;
            });

            var total = rows.reduce(function (sum, r) { return sum + (Number(r.amountSum) || 0); }, 0);
            $('listSummary').textContent = rows.length + (rows.length === 1 ? ' πληρωμή' : ' πληρωμές') + ' · σύνολο ' + money(total);

            box.textContent = '';
            if (rows.length === 0) {
                box.appendChild(el('p', 'empty', 'Δεν υπάρχουν πληρωμές σε αυτό το διάστημα.'));
                return;
            }
            rows.forEach(function (r) {
                var item = el('button', 'item');
                item.type = 'button';
                var line1 = el('div', 'line1');
                line1.appendChild(el('span', 'name', r.transactorName || ''));
                line1.appendChild(el('span', 'amount', money(r.amountSum)));
                item.appendChild(line1);
                var details = [displayDate(r.transDate)];
                if (r.transRefCode) details.push(r.transRefCode);
                if (r.etiology) details.push(r.etiology);
                item.appendChild(el('div', 'line2', details.join(' · ')));
                item.addEventListener('click', function () { startEdit(r); });
                box.appendChild(item);
            });
        } catch (e) {
            if (seq !== state.listSeq) return;
            $('listSummary').textContent = '';
            handleError(e);
        }
    }

    var searchTimer = null;

    function onListSearch() {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(loadList, 500);
    }

    // ---------- wiring ----------

    function init() {
        $('loginForm').addEventListener('submit', onLogin);
        $('logoutBtn').addEventListener('click', onLogout);
        $('companySelect').addEventListener('change', onCompanyChange);
        $('tabFormBtn').addEventListener('click', function () { showTab('form'); });
        $('tabListBtn').addEventListener('click', function () { showTab('list'); });

        $('supplierBtn').addEventListener('click', openSupplierPicker);
        $('supplierCloseBtn').addEventListener('click', closeSupplierPicker);
        $('supplierSearch').addEventListener('input', renderSuppliers);

        $('payForm').addEventListener('submit', onSave);
        $('deleteBtn').addEventListener('click', onDelete);
        $('cancelEditBtn').addEventListener('click', onCancelEdit);

        $('listFrom').addEventListener('change', loadList);
        $('listTo').addEventListener('change', loadList);
        $('listSearch').addEventListener('input', onListSearch);

        startServerWatch();

        if (store.get(KEY.access) && store.get(KEY.refresh)) enterApp();
        else showLogin('');
    }

    init();
})();
