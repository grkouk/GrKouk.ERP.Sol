var indPgLib = (function () {
    let indexPageDefinition;
    let colDefs;
    let actionColDefs;
    let actionColSubDefs;
    let actionMobileColDefs;
    let handlersToRegister;
    let currencyFormatter;
    let numberFormatter;

    //cached references=================================
    const $pageIndex = $('#pageIndex');
    const $pageSize = $('#PageSize');
    const $totalPages = $('#totalPages');
    const $totalRecords = $('#totalRecords');
    const $pagingInfo = $("[name=PagingInfo]");
    const $filtersVisible = $('#filtersVisible');
    const $filterCollapse = $('#filterCollapse');
    //Support functions===============================================
    const selectAllRowsColumnHtml = () => {
        let cl = '<th name="selectAllRowsColumn"> <label class="custom-control custom-checkbox"> ';
        cl += ' <input type="checkbox" class="custom-control-input" name="checkAllRows" >';
        cl += '<span class="custom-control-indicator"></span></label></th>';
        return cl;
    };
    const selectOneRowColumnHtml = (itemId) => {
        let cl = '<td name="selectRowColumn"> <label class="custom-control custom-checkbox">  ';
        cl += ' <input type="checkbox" class="custom-control-input" name="checkTableRow" ';
        cl += `data-itemId="${itemId}">`;
        cl += '<span class="custom-control-indicator"></span></label></td>';
        return cl;
    };
    const getTableCurrentSort = () => {
        try {
            let cSort = $("#currentSort").val();

            return cSort;
        } catch (e) {
            return "";
        }
    };
    const setTableCurrentSort = (currentSort) => {
        $('#currentSort').val(currentSort);
    };
    const countSelectedRows = () => {
        const $rowSelectors = $('input[name=checkTableRow]');
        var selectedRows = $rowSelectors.filter(':checked');
        return selectedRows.length;
    };
    const rowSelectorsUi = () => {
        var $selectSingleSelectors = $('td[name=selectRowColumn]');
        var $selectAllSelector = $('th[name=selectAllRowsColumn]');
        var $selectedRowsActionsMenu = $('#SelectedRowsActionsMenu');
        var $selectedRowsActionsLink = $('#ddSelectedRowsActions');
        var $rowSelectorsToggle = $('#rowSelectorsToggle');
        var $rowSelectorsVisible = $('#rowSelectorsVisible');

        var selectorsVisible = $rowSelectorsVisible.val();
        var newSelectorsVisible =
            (selectorsVisible == 'True' || selectorsVisible == 'true' || selectorsVisible === true)
                ? 'True'
                : 'False';
        if (newSelectorsVisible == 'True') {
            $selectAllSelector.removeAttr('hidden');
            $selectSingleSelectors.removeAttr('hidden');
            $selectAllSelector.show();
            $selectSingleSelectors.show();
            $rowSelectorsVisible.val(true);
            $rowSelectorsToggle.text('Hide Row Selectors');
            $selectedRowsActionsMenu.removeAttr('hidden');
            $selectedRowsActionsLink.addClass('disabled');
        } else {
            $selectAllSelector.hide();
            $selectSingleSelectors.hide();
            $rowSelectorsVisible.val(false);
            $rowSelectorsToggle.text('Show Row Selectors');
            $selectedRowsActionsMenu.attr('hidden', 'true');
        }
    };
    const handleSelectedRowsUi = () => {
        var $rowSelectors = $('input[name=checkTableRow]');
        var $checkAllRows = $('input[name=checkAllRows]');
        var $selectedRowsActionsLink = $('#ddSelectedRowsActions');
        var allRows = $rowSelectors.length;
        var selectedRowsCount = $rowSelectors.filter(':checked').length;
        if (selectedRowsCount > 0) {
            $selectedRowsActionsLink.removeClass('disabled');
            //$selectedRowsActionsLink.prop('disabled', 'false');
        } else {
            $selectedRowsActionsLink.addClass('disabled');
        }
        if (selectedRowsCount == allRows) {
            $checkAllRows.prop('checked', true);
        } else {
            $checkAllRows.prop('checked', false);
        }
    };
    const rowSelectorsToggleHandler = () => {
        const $selectSingleSelectors = $('td[name=selectRowColumn]');
        const $selectAllSelector = $('th[name=selectAllRowsColumn]');
        const $rowSelectorsVisible = $('#rowSelectorsVisible');
        const $rowSelectorsToggle = $('#rowSelectorsToggle');
        const $selectedRowsActionsMenu = $('#SelectedRowsActionsMenu');
        const $selectedRowsActionsLink = $('#ddSelectedRowsActions');

        var selectorsVisible = $rowSelectorsVisible.val();
        var newSelectorsVisible =
            (selectorsVisible == 'True' || selectorsVisible == 'true' || selectorsVisible === true)
                ? 'False'
                : 'True';
        if (newSelectorsVisible == 'True') {
            $selectAllSelector.removeAttr('hidden');
            $selectSingleSelectors.removeAttr('hidden');
            $selectAllSelector.show();
            $selectSingleSelectors.show();
            $rowSelectorsVisible.val(true);
            $rowSelectorsToggle.text('Hide Row Selectors');
            $selectedRowsActionsMenu.removeAttr('hidden');
            $selectedRowsActionsLink.addClass('disabled');
        } else {
            $selectAllSelector.hide();
            $selectSingleSelectors.hide();
            $rowSelectorsVisible.val(false);
            $rowSelectorsToggle.text('Show Row Selectors');
            $selectedRowsActionsMenu.attr('hidden', 'true');
        }
    };
    const handleFiltersUi = () => {
        var filterVisible = $filtersVisible.val();
        if (filterVisible === 'True' || filterVisible === 'true') {
            $filterCollapse.collapse('show');
        } else {
            $filterCollapse.collapse('hide');
        }
    };
    const reallyIsNaN = (x) => {
        return x !== x;
    };
    const isEmpty = inputObject => {
        return Object.keys(inputObject).length === 0;
    };
    //================================================================

    const createHeaderColumn = (item) => {
        let curSortUndefined = false;
        let curSortAr = [];
        var curSort = getTableCurrentSort();
        if (curSort === undefined || curSort === null || curSort.length === 0) {
            curSortUndefined = true;
        } else {
            curSortAr = curSort.split(":");
        }
        var tdColHead = '';
        if (item.sortKey.length !== 0) {
            let colHtml = "";
            colHtml = `<th class="${item.headerClass}"> `;
            colHtml += `<a href="#" role="button" name="SortHeader" tabindex="-1" `;
            colHtml += `data-sortkey="${item.sortKey}" data-sorttype="${item.sortType}">  `;
            colHtml += `${item.header}  `;
            colHtml += `</a>  `;
            if (curSortUndefined) {
                colHtml += `<i class="" name="SortIcon"></i>  `;
            } else {
                if (curSortAr[0] === item.sortKey) {

                    var newSortIconType = curSortAr[1] === 'asc' ? '-down' : '-up';
                    var iconSortType = "fas fa-sort-" + item.sortType + newSortIconType;
                    colHtml += `<i class="${iconSortType}" name="SortIcon"></i>  `;
                } else {
                    colHtml += `<i class="" name="SortIcon"></i>  `;
                }
            }

            colHtml += `</a>  `;
            tdColHead = colHtml;
        } else {
            tdColHead = $('<th>').text(item.header).addClass(item.headerClass);
        }
        return tdColHead;
    };
    const createValueColumn = (col, value) => {
        let $tdCol = $('<td>');
        if (col.responseKey) {
            if (col.remoteReference.isRemoteReference) {
                let valueKey = col.remoteReference.valueKey;
                var remoteId = value[valueKey];
                let remoteUrl = `${col.remoteReference.url}${remoteId}`;
                var $aLink = `<a href="${remoteUrl}" target="_blank">${value[col.responseKey]}</a>  `;
                //$tdCol.innerhtml ($aLink);
                //$($aLink).appendTo($tdCol);
                $tdCol.html($aLink);
            } else {
                switch (col.columnFormat) {
                    case 't':
                        $tdCol.text(value[col.responseKey]);
                        break;
                    case 'd':
                        $tdCol.text(moment(value[col.responseKey]).format('DD/MM/YYYY'));
                        break;
                    case 'c':
                        $tdCol.text(currencyFormatter.format(value[col.responseKey]));
                        $tdCol.attr('data-actualValue', value[col.responseKey]);
                        break;
                    default:
                }
            }

        } else {
            switch (col.columnFormat) {
                case 't':
                    break;
                case 'd':
                    break;
                case 'c':
                    $tdCol.attr('data-actualValue', 0);
                    break;
                default:
            }
        }
        $tdCol.addClass(col.class);
        return $tdCol;
    };
    const createColumnAction = (col, value) => {
        let actionHtml = '';
        switch (col.actionType) {
            case 'defaultAction':
                actionHtml += `<a href="${col.url}${value[col.valueKey]}">`;
                actionHtml += col.icon;
                actionHtml += '</a> |';
                break;

            default:
        }
        return actionHtml;
    };
    const createColumnSubAction = (col, value) => {
        let actionHtml = '';
        let visibility = true;
        if (col.visibility === 'condition') {
            visibility = false;
            if (!isEmpty(col.condition)) {


                let amnt1 = parseFloat(value[col.condition.val1Key]);
                let amnt2 = parseFloat(value[col.condition.val2Key]);
                let diffAmount = amnt1 - amnt2;
                switch (col.condition.operator) {
                    case 'notZero':
                        if (diffAmount !== 0) {
                            visibility = true;
                        }
                        break;
                    default:
                }
            }
        }
        if(visibility) {
            switch (col.actionType) {
                case 'defaultAction':
                    actionHtml += `<a class="dropdown-item" href="${col.url}${value[col.valueKey]}">`;
                    break;
                case 'newWindowAction':
                    actionHtml += `<a class="dropdown-item" target="_blanc" href="${col.url}${value[col.valueKey]}">`;
                    break;
                case 'eventAction':
                    actionHtml += `<a class="dropdown-item" href="#"`;
                    if (col.elementName) {
                        actionHtml += ` name=${col.elementName}`;
                    }
                    actionHtml += ` data-docid=${value[col.valueKey]}`;
                    actionHtml += '>';
                    break;
                case 'modalSelectorEventAction':
                    actionHtml += `<a class="dropdown-item" href="#" data-toggle="modal"`;
                    if (col.elementName) {
                        actionHtml += ` name=${col.elementName}`;
                    }
                    if (col.selectorTarget) {
                        actionHtml += ` data-target=${col.selectorTarget}`;
                    }
                    if (col.selectorType) {
                        actionHtml += ` data-selectorType=${col.selectorType}`;
                    }
                    actionHtml += ` data-docid=${value[col.valueKey]}`;
                    actionHtml += '>';
                    break;
                default:
            }
            actionHtml += col.icon;
            actionHtml += col.text;
            actionHtml += '</a>';
        }
        return actionHtml;
    };
    const createMobileColumnAction = (col, value) => {
        let actionHtml = '';
        let visibility = true;
        if (col.visibility === 'condition') {
            visibility = false;
            if (!isEmpty(col.condition)) {
                let amnt1 = parseFloat(value[col.condition.val1Key]);
                let amnt2 = parseFloat(value[col.condition.val2Key]);
                let diffAmount = amnt1 - amnt2;
                switch (col.condition.operator) {
                    case 'notZero':
                        if (diffAmount !== 0) {
                            visibility = true;
                        }
                        break;
                    default:
                }
            }
        }
        if (visibility) {
            switch (col.actionType) {
                case 'defaultAction':
                    actionHtml += `<a class="dropdown-item" href="${col.url}${value[col.valueKey]}">`;
                    break;
                case 'newWindowAction':
                    actionHtml += `<a class="dropdown-item" target="_blanc" href="${col.url}${value[col.valueKey]}">`;
                    break;
                case 'eventAction':
                    actionHtml += `<a class="dropdown-item" href="#"`;
                    if (col.elementName) {
                        actionHtml += ` name=${col.elementName}`;
                    }
                    actionHtml += ` data-docid=${value[col.valueKey]}`;
                    actionHtml += '>';
                    break;
                case 'modalSelectorEventAction':
                    actionHtml += `<a class="dropdown-item" href="#" data-toggle="modal"`;
                    if (col.elementName) {
                        actionHtml += ` name=${col.elementName}`;
                    }
                    if (col.selectorTarget) {
                        actionHtml += ` data-target=${col.selectorTarget}`;
                    }
                    if (col.selectorType) {
                        actionHtml += ` data-selectorType=${col.selectorType}`;
                    }
                    actionHtml += ` data-docid=${value[col.valueKey]}`;
                    actionHtml += '>';
                    break;
                default:
            }
            actionHtml += col.icon;
            actionHtml += col.text;
            actionHtml += '</a>';
        }
        return actionHtml;
    };
    const getIndexPageDefinition = () => {
        return indexPageDefinition;
    };
    const setIndexPageDefinition = (pgDefinition) => {
        indexPageDefinition = pgDefinition;
        colDefs = pgDefinition.colDefs;
        actionColDefs = pgDefinition.actionColDefs;
        currencyFormatter = pgDefinition.currencyFormatter;
        numberFormatter = pgDefinition.numberFormatter;
        actionColSubDefs = pgDefinition.actionColSubDefs;
        handlersToRegister = pgDefinition.handlersToRegister;
        actionMobileColDefs = pgDefinition.actionMobileColDefs;
    };

    const handlePagingUi = (totalPages, totalRecords, pageIndex, hasPrevious, hasNext) => {
        $totalPages.val(totalPages);
        $totalRecords.val(totalRecords);
        var pagingInfo = ` Page:${pageIndex} of ${totalPages} Total Items ${totalRecords}`;
        $pagingInfo.text(pagingInfo);

        if (hasPrevious) {
            $("#GoToFirst, #GoToPrevious").parent().removeClass("disabled");
        } else {
            $("#GoToFirst, #GoToPrevious").parent().addClass("disabled");
        }
        if (hasNext) {
            $("#GoToLast, #GoToNext").parent().removeClass("disabled");
        } else {
            $("#GoToLast, #GoToNext").parent().addClass("disabled");
        }
    };
    const getTableData = function (pgIndex, pgSize, sortData, dateRange, companyFlt, searchFlt, currencyFlt) {

        let uri = indexPageDefinition.uri;
        uri += `?pageIndex=${pgIndex}`;
        uri += `&pageSize=${pgSize}`;
        uri += `&companyFilter=${companyFlt}`;
        uri += `&dateRange=${dateRange}`;
        uri += `&sortData=${sortData}`;
        uri += `&searchFilter=${searchFlt}`;
        uri += `&displayCurrencyId=${currencyFlt}`;
        var timeout;
        return new Promise((resolve, reject) => {
            $.ajax({
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                type: "GET",
                url: uri,

                success: function (data) {
                    resolve(data);
                },
                error: function (error) {
                    reject(error);
                },
                beforeSend: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    timeout = setTimeout(function () {
                        $("#loadMe").modal({
                            backdrop: "static", //remove ability to close modal with click
                            keyboard: false, //remove option to close with keyboard
                            show: true //Display loader!
                        });
                    }, 1000);
                },
                complete: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    $("#loadMe").modal("hide");
                    setTimeout(function () {
                        console.log("Checking for open modals");
                        var isOpen = $("#loadMe").hasClass("show");
                        if (isOpen) {
                            console.log("There is an open Modal");
                            $("#loadMe").modal("hide");
                        } else {
                            console.log("No open modal");
                        }
                    }, 2000);
                },
            });
        });
    };
    const bindDataToTable = (result, pgIndex) => {
       
        handlePagingUi(result.totalPages, result.totalRecords, pgIndex, result.hasPrevious, result.hasNext);

        $("#myTable > tbody").empty();
        $("#myTable > thead").empty();

        var $trHead = $("<tr>");
        var $tdSelectCol = $(selectAllRowsColumnHtml());
        $trHead.append($tdSelectCol);
        colDefs.forEach(function (item) {

            $trHead.append($(createHeaderColumn(item)));

        });
        $trHead.append($('<th>'));
        $trHead.appendTo('#myTable > thead');

        $.each(result.data,
            function (index, value) {
                var itemId = value.id;
                var $tr = $('<tr>');
                var $tdSelectRowCol = $(selectOneRowColumnHtml(itemId));
                $tr.append($tdSelectRowCol);
                colDefs.forEach(function (col) {
                    $tr.append(createValueColumn(col, value));
                });

                let actionHtml = '';
                actionColDefs.forEach(function (col) {
                    actionHtml += createColumnAction(col, value);

                });
                if (actionColSubDefs.length > 0) {
                    actionHtml += '<a class="dropdown-toggle" role="button" id="dropdownMenuButton"';
                    actionHtml += 'data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    actionHtml += '<i class="fas fa-bars" style="color:slategray"></i></a>';
                    actionHtml += '<div class="dropdown-menu small" aria-labelledby="dropdownMenuButton">';

                    actionColSubDefs.forEach(function (col) {
                        actionHtml += createColumnSubAction(col, value);

                    });
                    actionHtml += '</div>';
                }
                let mobileHtml = '';
                if (actionMobileColDefs.length > 0) {
                   
                    mobileHtml += '<a class="dropdown-toggle" role="button" id="dropdownMenuButton"';
                    mobileHtml += 'data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                    mobileHtml += '<i class="fas fa-bars" style="color:slategray"></i></a>';
                    mobileHtml += '<div class="dropdown-menu small" aria-labelledby="dropdownMenuButton">';

                    actionMobileColDefs.forEach(function (col) {
                        mobileHtml += createMobileColumnAction (col, value);

                    });
                    mobileHtml += '</div>';
                }
                var actionsCol = `<td class="small text-center d-none d-lg-table-cell">${actionHtml}</td>`;
                var mobileCol = `<td class="small text-center d-table-cell d-sm-table-cell d-md-table-cell d-lg-none">${mobileHtml}</td>`;
                $tr.append(actionsCol);
                $tr.append(mobileCol);
                $tr.appendTo('#myTable > tbody');
            });
        handlersToRegister.forEach(function (item) {

            $(item.selector).on(item.event, item.handler);

        });  
    };

    const refreshTableData = () => {
        var pageIndexVal = parseInt($("#pageIndex").val());

        var pageIndex = reallyIsNaN(pageIndexVal) ? 1 : pageIndexVal;
        $("#pageIndex").val(pageIndex);

        var pageSize =
            $("#PageSize").val() == null || $("#PageSize").val().length == 0 ? 10 : parseInt($("#PageSize").val());

        var companyFlt = $("#CompanyFilter").val();
        var datePeriod = $("#DatePeriodFilter").val();
        var sortData = $("#currentSort").val();
        var searchFlt = $(".search_input").val();
        var $dcId = $("#CurrencySelector");

        var currencyFlt = $dcId.val() == null || $dcId.val().length == 0 ? 1 : parseInt($dcId.val());

        getTableData(pageIndex, pageSize, sortData, datePeriod, companyFlt, searchFlt, currencyFlt)
            .then((data) => {
                bindDataToTable(data, pageIndex);
            })
            .catch((error) => {
                console.log(error);
            });
    }

    const addPagerElementEventListeners = () => {
        let pagerElements = document.getElementsByClassName("page-link");
        Array.from(pagerElements).forEach((item) => {
            item.addEventListener("click", (event) => {
                console.log("From pager event handler");
                let pagerAction = event.currentTarget.dataset.pageraction;
                if (pagerAction === undefined) {
                    return;
                }

                let curPageIndex;
                switch (pagerAction) {
                    case "GoToFirst":
                        $pageIndex.val(1);
                        break;
                    case "GoToPrevious":
                        curPageIndex = parseInt($pageIndex.val());
                        curPageIndex -= 1;
                        $pageIndex.val(curPageIndex);
                        break;
                    case "GoToNext":
                        curPageIndex = parseInt($pageIndex.val());
                        curPageIndex += 1;
                        $pageIndex.val(curPageIndex);
                        break;
                    case "GoToLast":
                        curPageIndex = parseInt($totalPages.val());
                        $pageIndex.val(curPageIndex);
                        break;
                    default:
                }
            });
        });
    };

    return {
        getIndexPageDefinition: getIndexPageDefinition,
        setIndexPageDefinition: setIndexPageDefinition,
        refreshData: refreshTableData,
        addPagerElementEventListeners: addPagerElementEventListeners,
        getTableCurrentSort: getTableCurrentSort,
        setTableCurrentSort: setTableCurrentSort,
        countSelectedRows: countSelectedRows,
        rowSelectorsUi: rowSelectorsUi,
        handleSelectedRowsUi: handleSelectedRowsUi,
        rowSelectorsToggleHandler: rowSelectorsToggleHandler,
        handleFiltersUi: handleFiltersUi

    };
})();
