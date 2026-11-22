// Shared behaviour of the Syncfusion index pages (one grid per page). The page only describes
// itself; this library does the rest:
//   - remembers filters, paging, sorting and panel toggles in localStorage
//   - sends the filters with every grid request (and the antiforgery token)
//   - search box (Enter, icon, 600ms after typing, clear button)
//   - filters panel and row selectors toggles, Delete Selected with confirmation
//   - page and grand totals, display-currency formatting, grid fitted to the window
//   - row menu actions: elements with data-action="name" data-id="id" call config.rowActions[name](id)
//
// Usage (see Pages/Transactions/BuyMaterialsDoc/IndexSf.cshtml):
//   <script> const sfIdx = sfIndexPage.create({...}); </script>   // before <ejs-scripts>
//   <ejs-scripts></ejs-scripts>
//   <script> document.addEventListener('DOMContentLoaded', () => sfIdx.start()); </script>
//
// create() must run before <ejs-scripts>: it defines the global handlers that the markup names
// (sfGridLoad, sfGridActionComplete, sfGridActionFailure, sfGridDataBound, sfSelectionChanged,
// sfCurrencyAccessor, sfFilterChanged, sfDatePeriodChanged, sfCustomDateChanged).
// start() needs the created controls.
//
// Config:
//   gridId, storageKey                    required
//   defaults: { pageSize, sortField, sortDirection, dateRange, companyFilter, displayCurrencyId, ... }
//   combos: [{ id, key, number? }]        filter combos besides the period (key = filter name sent)
//   period: true                          DatePeriodFilter with FromDateFilter/ToDateFilter
//   currencies: [...]                     CurrencySelector items; enables display-currency formatting
//   deleteUrl, deleteNoun                 Delete Selected handler and the noun for its message
//   totalsUrl, totals: [{ key, field, grandField }]   page totals from rows[field], grand from response[grandField]
//   rowActions: { name: (id) => ... }
//   loadErrorText
window.sfIndexPage = (() => {
    const pad2 = (n) => String(n).padStart(2, '0');
    const toIsoDate = (d) => d ? `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}` : '';
    const fromIsoDate = (s) => {
        if (!s) return null;
        const [y, m, d] = s.split('-').map(Number);
        return new Date(y, m - 1, d);
    };
    const formatDate = (value) => {
        if (!value) return '';
        const d = value instanceof Date ? value : new Date(value);
        return isNaN(d) ? '' : `${pad2(d.getDate())}/${pad2(d.getMonth() + 1)}/${d.getFullYear()}`;
    };
    const getToken = () => document.querySelector('input[name="__RequestVerificationToken"]').value;
    const getControl = (id) => {
        const el = document.getElementById(id);
        return el && el.ej2_instances ? el.ej2_instances[0] : null;
    };

    // POST JSON with the antiforgery token; resolves to { ok, result }.
    const postJson = async (url, body) => {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getToken() },
            body: JSON.stringify(body)
        });
        const result = await response.json().catch(() => ({}));
        return { ok: response.ok, result };
    };

    const create = (config) => {
        const gridId = config.gridId;
        const combos = config.combos || [];
        const totals = config.totals || [];
        const currencies = config.currencies || null;

        const defaults = Object.assign({
            pageSize: 10,
            sortField: '',
            sortDirection: 'Ascending',
            searchFilter: '',
            filtersVisible: true,
            rowSelectorsVisible: false
        }, config.period ? { dateRange: 'CURYEAR', fromDate: '', toDate: '' } : {},
            currencies ? { displayCurrencyId: 1 } : {},
            config.defaults || {});

        // #region Remembered settings
        const settings = (() => {
            try {
                return Object.assign({}, defaults, JSON.parse(localStorage.getItem(config.storageKey) || '{}'));
            } catch (e) {
                return Object.assign({}, defaults);
            }
        })();
        const saveSettings = () => {
            try {
                localStorage.setItem(config.storageKey, JSON.stringify(settings));
            } catch (e) {
                // Storage unavailable (private window etc.): settings just are not remembered.
            }
        };
        // #endregion

        // #region Formatting
        const currentCulture = document.documentElement.lang || navigator.language;
        let currencyFormatter;
        const setCurrencyFormatter = () => {
            const currency = (currencies || []).find(c => c.id === Number(settings.displayCurrencyId)) || { code: 'EUR' };
            try {
                currencyFormatter = new Intl.NumberFormat(currency.displayLocale || config.culture || currentCulture,
                    { style: 'currency', currency: currency.code, minimumFractionDigits: 2 });
            } catch (e) {
                currencyFormatter = new Intl.NumberFormat(config.culture || currentCulture, { minimumFractionDigits: 2 });
            }
        };
        setCurrencyFormatter();
        const formatCurrency = (value) => currencyFormatter.format(Number(value) || 0);
        // #endregion

        const getGrid = () => getControl(gridId);
        // Grid height follows the window: rows scroll inside, totals and footer stay on screen.
        const fitGrid = () => requestAnimationFrame(() => sfGridFit.fit(gridId));

        // #region Filters
        // A custom period without both dates falls back to the default period.
        const effectiveDateRange = () =>
            settings.dateRange === 'CUSTOM' && !(settings.fromDate && settings.toDate)
                ? defaults.dateRange
                : (settings.dateRange || defaults.dateRange);

        const currentFilters = () => {
            const filters = { searchFilter: settings.searchFilter ?? '' };
            if (config.period) {
                filters.dateRange = effectiveDateRange();
                filters.fromDate = settings.fromDate || null;
                filters.toDate = settings.toDate || null;
            }
            combos.forEach(c => {
                filters[c.key] = c.number ? Number(settings[c.key]) || 0 : String(settings[c.key] ?? '');
            });
            if (currencies) {
                filters.displayCurrencyId = Number(settings.displayCurrencyId) || 1;
            }
            return filters;
        };

        const buildQuery = () => {
            const query = new ej.data.Query();
            Object.entries(currentFilters()).forEach(([key, value]) => query.addParams(key, value ?? ''));
            return query;
        };
        // #endregion

        // #region Totals
        const setCurrencyText = (id, value) => {
            const el = document.getElementById(id);
            if (el) el.textContent = formatCurrency(value);
        };

        // Grand totals of everything that matches the filters (not just the current page).
        const loadGrandTotals = async () => {
            if (!config.totalsUrl) {
                return;
            }
            try {
                const { ok, result } = await postJson(config.totalsUrl, currentFilters());
                if (!ok) {
                    throw new Error(result.error || 'Totals request failed');
                }
                totals.forEach(t => setCurrencyText(`grand${t.key}`, result[t.grandField]));
            } catch (err) {
                console.log(err);
                toastr.error('Could not load the totals', 'Error');
            }
        };
        // #endregion

        // #region Reload
        const selectionChanged = () => {
            const grid = getGrid();
            const menu = document.getElementById('ddSelectedRowsActions');
            if (grid && menu) {
                menu.classList.toggle('disabled', grid.getSelectedRecords().length === 0);
            }
        };

        // Reload the grid from page 1 (and the grand totals) with the current filters.
        const refreshAll = () => {
            const grid = getGrid();
            grid.clearSelection();
            grid.query = buildQuery();
            grid.goToPage(1);
            grid.refresh();
            selectionChanged();
            loadGrandTotals();
        };

        // Reload the current page and the grand totals (after a row action).
        const reloadData = () => {
            getGrid().refresh();
            loadGrandTotals();
        };

        const showCustomDates = () => {
            const el = document.getElementById('customDates');
            if (el) {
                el.classList.toggle('visible', settings.dateRange === 'CUSTOM');
            }
            fitGrid();
        };
        // #endregion

        // #region Handlers named in the markup (must exist before <ejs-scripts>)

        // Grid "load" runs before the first request: add the antiforgery header,
        // the filter parameters and the remembered paging/sorting.
        window.sfGridLoad = function () {
            this.dataSource.dataSource.headers = [{ 'RequestVerificationToken': getToken() }];
            this.query = buildQuery();
            this.pageSettings.pageSize = settings.pageSize;
            // The pager copied pageSettings before "load" fired, so update its copy too
            // or the page size dropdown keeps showing the markup value.
            if (this.pagerModule && this.pagerModule.pagerObj) {
                this.pagerModule.pagerObj.pageSize = settings.pageSize;
            }
            if (settings.sortField) {
                this.sortSettings.columns = [{ field: settings.sortField, direction: settings.sortDirection || 'Ascending' }];
            }
            if (this.columns[0] && this.columns[0].type === 'checkbox') {
                this.columns[0].visible = !!settings.rowSelectorsVisible;
            }
        };

        window.sfGridActionComplete = function (args) {
            if (args.requestType === 'paging') {
                settings.pageSize = this.pageSettings.pageSize;
                saveSettings();
            }
            if (args.requestType === 'sorting') {
                const sorted = this.sortSettings.columns[0];
                settings.sortField = sorted ? sorted.field : '';
                settings.sortDirection = sorted ? sorted.direction : '';
                saveSettings();
            }
        };

        window.sfGridActionFailure = function (args) {
            console.log(args);
            toastr.error(config.loadErrorText || 'Could not load the data', 'Error');
        };

        // Page totals from the rows on screen (already in the display currency).
        window.sfGridDataBound = function () {
            if (totals.length > 0) {
                const rows = this.getCurrentViewRecords();
                const sum = (key) => rows.reduce((total, row) => total + (Number(row[key]) || 0), 0);
                totals.forEach(t => setCurrencyText(`page${t.key}`, sum(t.field)));
            }
            fitGrid();
        };

        window.sfSelectionChanged = () => selectionChanged();

        // Column value accessor for amounts: valueAccessor="sfCurrencyAccessor".
        window.sfCurrencyAccessor = (field, data) => formatCurrency(data[field]);

        window.sfFilterChanged = (args) => {
            // Ignore the programmatic changes made when the remembered filters are restored.
            if (args && args.isInteracted === false) {
                return;
            }
            combos.forEach(c => {
                const control = getControl(c.id);
                if (control) settings[c.key] = control.value ?? defaults[c.key];
            });
            if (currencies) {
                settings.displayCurrencyId = Number(getControl('CurrencySelector').value) || 1;
                setCurrencyFormatter();
            }
            saveSettings();
            refreshAll();
        };

        window.sfDatePeriodChanged = (args) => {
            if (args && args.isInteracted === false) {
                return;
            }
            settings.dateRange = getControl('DatePeriodFilter').value || defaults.dateRange;
            saveSettings();
            showCustomDates();
            // A custom period waits until both dates are picked.
            if (settings.dateRange !== 'CUSTOM' || (settings.fromDate && settings.toDate)) {
                refreshAll();
            }
        };

        window.sfCustomDateChanged = (args) => {
            if (args && args.isInteracted === false) {
                return;
            }
            settings.fromDate = toIsoDate(getControl('FromDateFilter').value);
            settings.toDate = toIsoDate(getControl('ToDateFilter').value);
            saveSettings();
            if (settings.fromDate && settings.toDate) {
                refreshAll();
            }
        };
        // #endregion

        // #region Wiring that needs the created controls
        const restoreFilters = () => {
            // Set the remembered values without firing change (the grid reads them in sfGridLoad).
            if (config.period) {
                getControl('DatePeriodFilter').value = settings.dateRange;
                getControl('FromDateFilter').value = fromIsoDate(settings.fromDate);
                getControl('ToDateFilter').value = fromIsoDate(settings.toDate);
            }
            combos.forEach(c => {
                const control = getControl(c.id);
                if (control) control.value = c.number ? Number(settings[c.key]) : String(settings[c.key] ?? '');
            });
            if (currencies) {
                getControl('CurrencySelector').value = Number(settings.displayCurrencyId) || 1;
            }
            const searchEl = document.getElementById('SearchFilter');
            if (searchEl) searchEl.value = settings.searchFilter;
            showCustomDates();
        };

        const wireFiltersToggle = () => {
            const collapseEl = document.getElementById('filterCollapse');
            const toggleEl = document.getElementById('filtersToggle');
            if (!collapseEl || !toggleEl) return;
            const collapse = new bootstrap.Collapse(collapseEl, { toggle: false });
            collapseEl.addEventListener('shown.bs.collapse', fitGrid);
            collapseEl.addEventListener('hidden.bs.collapse', fitGrid);
            if (!settings.filtersVisible) {
                collapseEl.classList.remove('show');
                toggleEl.textContent = 'Show Filters';
            }
            toggleEl.addEventListener('click', (e) => {
                e.preventDefault();
                settings.filtersVisible = !settings.filtersVisible;
                saveSettings();
                settings.filtersVisible ? collapse.show() : collapse.hide();
                toggleEl.textContent = settings.filtersVisible ? 'Hide Filters' : 'Show Filters';
            });
        };

        const wireRowSelectorsToggle = () => {
            const toggleEl = document.getElementById('rowSelectorsToggle');
            if (!toggleEl) return;
            const setText = () => {
                toggleEl.textContent = settings.rowSelectorsVisible ? 'Hide Row Selectors' : 'Show Row Selectors';
            };
            setText();
            toggleEl.addEventListener('click', (e) => {
                e.preventDefault();
                settings.rowSelectorsVisible = !settings.rowSelectorsVisible;
                saveSettings();
                setText();
                const grid = getGrid();
                if (!settings.rowSelectorsVisible) {
                    grid.clearSelection();
                    selectionChanged();
                }
                grid.columns[0].visible = settings.rowSelectorsVisible;
                grid.refreshColumns();
            });
        };

        // Search: on Enter, on the icon, or 600ms after typing stops.
        const wireSearch = () => {
            const searchEl = document.getElementById('SearchFilter');
            if (!searchEl) return;
            let timer;
            const runSearch = () => {
                clearTimeout(timer);
                if (settings.searchFilter === searchEl.value) {
                    return;
                }
                settings.searchFilter = searchEl.value;
                saveSettings();
                refreshAll();
            };
            searchEl.addEventListener('keyup', (e) => {
                clearTimeout(timer);
                if (e.key === 'Enter') {
                    runSearch();
                } else {
                    timer = setTimeout(runSearch, 600);
                }
            });
            const button = document.getElementById('SearchButton');
            if (button) button.addEventListener('click', (e) => {
                e.preventDefault();
                runSearch();
            });
            const clear = document.getElementById('SearchClear');
            if (clear) clear.addEventListener('click', (e) => {
                e.preventDefault();
                searchEl.value = '';
                searchEl.focus();
                runSearch();
            });
        };

        // Delete selected rows (after confirmation, _DeleteConfirmationPartial).
        const wireDeleteSelected = () => {
            const link = document.getElementById('deleteSelectedRows');
            const modalEl = document.getElementById('confirmDeleteModal');
            if (!link || !modalEl || !config.deleteUrl) return;
            const confirmModal = new bootstrap.Modal(modalEl);
            link.addEventListener('click', (e) => {
                e.preventDefault();
                if (getGrid().getSelectedRecords().length > 0) {
                    confirmModal.show();
                }
            });
            document.getElementById('confirmDelete').addEventListener('click', async () => {
                confirmModal.hide();
                const ids = getGrid().getSelectedRecords().map(r => r.id);
                try {
                    const { ok, result } = await postJson(config.deleteUrl, { ids });
                    if (ok) {
                        toastr.success(`${result.deleted} ${config.deleteNoun || 'item(s)'} deleted`, 'Delete');
                        refreshAll();
                    } else {
                        toastr.error(result.error || 'Delete failed', 'Error');
                    }
                } catch (err) {
                    toastr.error('Delete failed', 'Error');
                }
            });
        };

        const wireRowActions = () => {
            const actions = config.rowActions || {};
            document.getElementById(gridId).addEventListener('click', (e) => {
                const actionEl = e.target.closest('[data-action]');
                if (!actionEl || !actions[actionEl.dataset.action]) {
                    return;
                }
                e.preventDefault();
                actions[actionEl.dataset.action](parseInt(actionEl.dataset.id, 10));
            });
        };

        let started = false;
        const start = () => {
            if (started) return;
            started = true;
            restoreFilters();
            loadGrandTotals();
            sfGridFit.watch(gridId);
            wireFiltersToggle();
            wireRowSelectorsToggle();
            wireSearch();
            wireDeleteSelected();
            wireRowActions();
        };
        // #endregion

        return {
            start,
            settings,
            currentFilters,
            formatCurrency,
            formatDate,
            getGrid,
            refreshAll,
            reloadData,
            postJson
        };
    };

    return { create, formatDate, postJson, getToken };
})();
