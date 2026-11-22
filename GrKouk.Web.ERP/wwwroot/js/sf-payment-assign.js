// Payoff and receipts/payments assignment for the sell and buy Syncfusion index pages.
// Drives the modal of Pages/Shared/Sf/_SfPaymentAssignModal.cshtml; the page passes its endpoints:
//
//   const payments = sfPaymentAssign.create({
//       page: sfIdx,                       // from sfIndexPage.create (formatting, filters, reload)
//       listUrl: '/api/GrKoukInfoApi/GetSelectorPaymentsToSuppliersItems',
//       saveUrl: '/api/Materials/AddBuyPaymentMappingList',
//       payoffUrl: '/api/materials/AddBuyPaymentMapping',
//       itemsNoun: 'payments'
//   });
//   payments.open(docId);  payments.payoff(docId);
//
// open() needs the modal in the DOM: call create() after DOMContentLoaded.
window.sfPaymentAssign = (() => {
    const create = (config) => {
        const page = config.page;
        const noun = config.itemsNoun || 'payments';
        const modalEl = document.getElementById('paymentAssignModal');
        const modal = new bootstrap.Modal(modalEl);
        const body = document.querySelector('#paTable > tbody');
        const spinner = document.getElementById('paSpinner');
        const state = { docId: 0, docAmount: 0, toCover: 0, covered: 0, items: [] };
        const displayCurrencyId = () => Number(page.settings.displayCurrencyId) || 1;

        const renderTotals = () => {
            document.getElementById('paDocAmount').textContent = page.formatCurrency(state.docAmount);
            document.getElementById('paToCover').textContent = page.formatCurrency(state.toCover);
            document.getElementById('paCovered').textContent = page.formatCurrency(state.covered);
            document.getElementById('paRemaining').textContent = page.formatCurrency(state.toCover - state.covered);
        };

        const renderItems = () => {
            body.innerHTML = '';
            state.items.forEach((item, index) => {
                const tr = document.createElement('tr');
                const cells = [
                    { text: page.formatDate(item.transDate), cls: 'd-none d-md-table-cell' },
                    { text: item.docSeriesCode ?? '' },
                    { text: page.formatCurrency(item.totalAmount), cls: 'text-end' },
                    { text: page.formatCurrency(item.amountUsedInPayments), cls: 'text-end' },
                    { text: item.companyCode ?? '' },
                    { text: page.formatCurrency(item.reserved), cls: 'text-end text-primary' }
                ];
                cells.forEach(c => {
                    const td = document.createElement('td');
                    td.textContent = c.text;
                    if (c.cls) td.className = c.cls;
                    tr.appendChild(td);
                });
                const tdCheck = document.createElement('td');
                tdCheck.className = 'text-center';
                const check = document.createElement('input');
                check.type = 'checkbox';
                check.className = 'form-check-input';
                check.checked = item.reserved > 0;
                check.dataset.index = index;
                tdCheck.appendChild(check);
                tr.appendChild(tdCheck);
                body.appendChild(tr);
            });
        };

        // Checking an item reserves what is still available on it, up to what remains to cover.
        body.addEventListener('change', (e) => {
            const check = e.target.closest('input[type=checkbox]');
            if (!check) {
                return;
            }
            const item = state.items[Number(check.dataset.index)];
            if (check.checked) {
                const remaining = state.toCover - state.covered;
                const available = item.totalAmount - item.amountUsedInPayments;
                const toReserve = Math.min(available, remaining);
                if (toReserve <= 0) {
                    check.checked = false;
                    return;
                }
                item.reserved = toReserve;
                state.covered += toReserve;
            } else {
                state.covered -= item.reserved;
                item.reserved = 0;
            }
            check.closest('tr').children[5].textContent = page.formatCurrency(item.reserved);
            renderTotals();
        });

        const open = async (docId) => {
            state.docId = docId;
            state.items = [];
            body.innerHTML = '';
            spinner.style.display = '';
            modal.show();
            const params = new URLSearchParams({
                pageIndex: 1,
                pageSize: 100,
                companyFilter: '',
                sortData: '',
                documentId: docId,
                searchFilter: '',
                displayCurrencyId: displayCurrencyId()
            });
            try {
                const response = await fetch(`${config.listUrl}?${params}`);
                const result = await response.json().catch(() => ({}));
                if (!response.ok) {
                    toastr.error(result.error || `Could not load the ${noun}`, 'Error');
                    return;
                }
                state.docAmount = Number(result.sumOfAmount) || 0;
                state.toCover = state.docAmount - (Number(result.sumOfPayedAmount) || 0);
                state.covered = 0;
                state.items = (result.data || []).map(r => Object.assign({}, r, {
                    totalAmount: Number(r.totalAmount) || 0,
                    amountUsedInPayments: Number(r.amountUsedInPayments) || 0,
                    reserved: 0
                }));
                renderTotals();
                renderItems();
            } catch (err) {
                console.log(err);
                toastr.error(`Could not load the ${noun}`, 'Error');
            } finally {
                spinner.style.display = 'none';
            }
        };

        document.getElementById('paSave').addEventListener('click', async () => {
            const lines = state.items
                .filter(r => r.reserved > 0)
                .map(r => ({ receiptId: r.id, amountUsed: r.reserved }));
            try {
                const response = await fetch(config.saveUrl, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        docId: state.docId,
                        displayCurrencyId: displayCurrencyId(),
                        paymentMappingLines: lines
                    })
                });
                const result = await response.json().catch(() => ({}));
                if (response.ok) {
                    toastr.success('Saved Mappings', 'Saved Mappings');
                    modal.hide();
                    page.reloadData();
                } else {
                    toastr.error(result.error || 'Save failed', 'Error');
                }
            } catch (err) {
                toastr.error('Save failed', 'Error');
            }
        });

        const payoff = async (docId) => {
            try {
                const response = await fetch(config.payoffUrl, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ ids: [docId] })
                });
                const result = await response.json().catch(() => ({}));
                if (response.ok) {
                    toastr.success(result.message || 'Done');
                    page.reloadData();
                } else {
                    toastr.error(result.errorMessage || result.error || 'Payoff failed', 'Error');
                }
            } catch (err) {
                toastr.error('Payoff failed', 'Error');
            }
        };

        return { open, payoff };
    };

    return { create };
})();
