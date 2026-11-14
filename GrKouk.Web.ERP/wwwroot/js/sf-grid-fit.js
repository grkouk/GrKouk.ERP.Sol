// Fits a Syncfusion grid to the window height: the grid's scrolling area gets the height that
// lets the whole page - everything above the grid, the grid with its pager, and everything after it
// (totals, footer) - fit in the window. The rows then scroll inside the grid while the column
// headers, the pager and the footer stay visible.
//
// Usage: sfGridFit.fit('GridId') whenever the space may have changed (grid dataBound,
// filters shown/hidden), and sfGridFit.watch('GridId') once for window resizes.
window.sfGridFit = (() => {
    const fit = (gridId, options = {}) => {
        const gridEl = document.getElementById(gridId);
        const grid = gridEl && gridEl.ej2_instances && gridEl.ej2_instances[0];
        const content = gridEl && gridEl.querySelector('.e-gridcontent');
        if (!grid || !content) {
            return;
        }
        const minHeight = options.minHeight ?? 300;
        const bottomGap = options.bottomGap ?? 0;

        // Positions in page coordinates, so the result doesn't depend on the current scroll.
        const contentTop = content.getBoundingClientRect().top + window.scrollY;
        const pager = gridEl.querySelector('.e-gridpager');
        const pagerHeight = pager ? pager.offsetHeight : 0;
        // Everything after the grid up to the end of the page (totals, footer, body padding).
        const afterHeight = Math.max(0,
            document.body.getBoundingClientRect().bottom - gridEl.getBoundingClientRect().bottom);

        const height = Math.max(minHeight,
            Math.floor(window.innerHeight - contentTop - pagerHeight - afterHeight - bottomGap));
        if (grid.height !== height) {
            grid.height = height;
        }
    };

    // Re-fit on window resize (debounced).
    const watch = (gridId, options = {}) => {
        let timer;
        window.addEventListener('resize', () => {
            clearTimeout(timer);
            timer = setTimeout(() => fit(gridId, options), 100);
        });
    };

    return { fit, watch };
})();
