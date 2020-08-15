//Index Page library functions
const indexPageLib = {
    pagerElementsClassName: "page-link",
    getPageElements: function () {
        let pageElements = document.getElementsByClassName(this.pagerElementsClassName);
        return pageElements;
    },
    addPagerElementEventListener: function () {
        let pagerElements = this.getPageElements();
        Array.from(pagerElements).forEach(item => {
            item.addEventListener('click',
                event => {
                    console.log("From pager event handler");
                    let pagerAction = event.currentTarget.dataset.pageraction;
                    if (pagerAction === undefined) {
                        return;
                    }
                    var pageIndex = document.getElementById('pageIndex');
                    var pageSize = document.getElementById('PageSize');
                    var totalPages = document.getElementById('totalPages');
                    var totalRecords = document.getElementById('totalRecords');
                    switch (pagerAction) {
                        case 'GoToFirst':
                            break;
                        case 'GoToPrevious':
                            break;
                        case 'GoToNext':
                            break;
                        case 'GoToLast':
                            break;
                        default:
                    }
                });
        });

    },
    setPagerElementsClassName: function (className) {
        this.pagerElementsClassName = className;
    }
};