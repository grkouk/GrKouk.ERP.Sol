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
                    var pageIndexEl = document.getElementById('pageIndex');
                    let pageIndex = pageIndexEl.value;
                    var pageSizeEl = document.getElementById('PageSize');
                    let pageSize = pageSizeEl.value;
                    var totalPagesEl = document.getElementById('totalPages');
                    let totalPages = totalPagesEl.value;
                    var totalRecordsEl = document.getElementById('totalRecords');
                    let totalRecords = totalRecordsEl.value;

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
    },
    gotoFirstPage: function () {

    },
    gotoLastPage: function () {

    },
    gotoNextPage: function () {

    },
    gotoPreviousPage: function () {

    },
    handlePagingUi: (totalPages, totalRecords, pageIndex, hasPrevious, hasNext) => {
        //$totalPages.val(totalPages);
        //$totalRecords.val(totalRecords);
        var pagingInfo =
            ` Page:${pageIndex} of ${totalPages} Total Items ${totalRecords}`;
        $('[name=PagingInfo]').text(pagingInfo);

        if (hasPrevious) {
            $('#GoToFirst, #GoToPrevious').parent().removeClass('disabled');
        } else {
            $('#GoToFirst, #GoToPrevious').parent().addClass('disabled');
        }
        if (hasNext) {
            $('#GoToLast, #GoToNext').parent().removeClass('disabled');
        } else {
            $('#GoToLast, #GoToNext').parent().addClass('disabled');
        }
    }
};