var indPgLib = (function () {
    let indexPageDefinition;
    let colDefs;
    let actionColDefs;
    let actionColSubDefs;
    let actionMobileColDefs;
    let tableHandlersToRegister;
    let pageHandlersToRegister;
    let currencyFormatter;
    let numberFormatter;
    let tableElementId;
    let currentTableSort;
    //Index page or selector 
    //if selector then there should not be a 1st column for selection 
    //Valid values are Index, TransactorSelector, ProductSelector
    let indexPageType;
    let spinnerLoaderShow;
    let spinnerLoaderHide;
    let spinnerLoaderIsVisible;

    let getTableCurrentSort;
    let setTableCurrentSort;
    const indexPageSpinnerShow = () => {
        $("#loadMe").modal({
            backdrop: "static",
            keyboard: false,
            show: true
        });
    };

    const indexPageSpinnerHide = () => {
        $("#loadMe").modal("hide");
    };
    const indexPageSpinnerIsVisible = () => {
        return $("#loadMe").hasClass("show");
    };
    const selectorSpinnerShow = () => {
        $('#SpinnerLoader').show();
    };
    const selectorSpinnerHide = () => { $('#SpinnerLoader').hide(); };
    const selectorSpinnerIsVisible = () => {
        return( $('#SpinnerLoader').is(':visible'));
    };
    //cached references=================================
    let $pageIndex = $("#pageIndex");
    let $pageSize = $("#PageSize");
    let $totalPages = $("#totalPages");
    let $companyFilter = $("#CompanyFilter");
    let $totalRecords = $("#totalRecords");
    let $pagingInfo = $("[name=PagingInfo]");
    let $filtersVisible = $("#filtersVisible");
    let $filterCollapse = $("#filterCollapse");
    let $selectedRowsActionsLink = $("#ddSelectedRowsActions");
    let $filtersToggle = $('#filtersToggle');
    let $datePeriodFilter = $("#DatePeriodFilter");
    let $currentSort = $("#currentSort");
    let $transactorTypeFilter = $('#TransactorTypeFilter');
    let $transactorId = $('#TransactorId');
    let $cfaId = $('#CfaId');
    let $showCarryOnFlt = $('#ShowCarryOnFlt');
    let $warehouseItemId = $('#WarehouseItemId');
    let $warehouseItemNatureFilter = $('#WarehouseItemNatureFilter');
    let $currencySelector = $("#CurrencySelector");
    let $diaryId = $("#DiaryId");
    //-------------------------------------------------------
    let companyFilterElement;
    let datePeriodFilterElement;
    let pageIndexElement;
    let pageSizeElement;
    let searchTextElement;
    let currencyFilterElement;
    let transactorTypeFilterElement;
    let transactorIdFilterElement;
    let cfaIdFilterElement;
    let warehouseItemIdFilterElement;
    let productNatureFilterElement;
    let tableCurrentSortElement;
    let diaryIdFilterElement;
    let showCarryOnFilterElement;

    let setFilterValues;
    const setSelectorFilterValues = () => {
        let flt = indexPageDefinition.getFilterValues();

        pageIndexElement = flt.pageIndexElement;
        pageSizeElement = flt.pageSizeElement;
        companyFilterElement = flt.companyFilterElement;
        datePeriodFilterElement = flt.datePeriodFilterElement;
        var sortData = getTableCurrentSort();

        tableCurrentSortElement = sortData;
        searchTextElement = flt.searchTextElement;
        currencyFilterElement = flt.currencyFilterElement;
        transactorTypeFilterElement = flt.transactorTypeFilterElement;
        transactorIdFilterElement = flt.transactorIdFilterElement;
        cfaIdFilterElement = flt.cfaIdFilterElement;
        warehouseItemIdFilterElement = flt.warehouseItemIdFilterElement;
        productNatureFilterElement = flt.productNatureFilterElement;
        diaryIdFilterElement = flt.diaryIdFilterElement;
        showCarryOnFilterElement = flt.showCarryOnFilterElement;
    };
    const setIndexPageFilterValues = () => {
        var pageIndexVal = parseInt($pageIndex.val());

        var pageIndex = reallyIsNaN(pageIndexVal) ? 1 : pageIndexVal;
        $pageIndex.val(pageIndex);

        pageIndexElement = pageIndex;

        var pageSize =
            $pageSize.val() == null || $pageSize.val().length == 0 ? 10 : parseInt($pageSize.val());

        pageSizeElement = pageSize;

        var companyFlt = $companyFilter.val();

        companyFilterElement = companyFlt;

        var datePeriod = $datePeriodFilter.val();

        datePeriodFilterElement = datePeriod;

        var sortData = getTableCurrentSort();

        tableCurrentSortElement = sortData;

        var searchFlt = $(".search_input").val();

        searchTextElement = searchFlt;

        var $dcId = $currencySelector;
        var currencyFlt = $dcId.val() == null || $dcId.val().length == 0 ? 1 : parseInt($dcId.val());

        currencyFilterElement = currencyFlt;

        var transTypeFlt = '';
        var transactorId = 0;
       
       
        if (!($transactorTypeFilter.val() === undefined)) {
            transTypeFlt = $transactorTypeFilter.val();
            transactorTypeFilterElement = transTypeFlt;
        } else {
            transactorTypeFilterElement = "";
        }

        if (!($transactorId.val() === undefined)) {
            transactorId = $transactorId.val();
            transactorIdFilterElement = transactorId;
        } else {
            transactorIdFilterElement = 0;
        }
        var cfaId = 0;
        if (!($cfaId.val() === undefined)) {
            cfaId = $cfaId.val();
            cfaIdFilterElement = cfaId;
        } else {
            cfaIdFilterElement = 0;
        }
        var showCarryOnValue=false;
        if (!($showCarryOnFlt === undefined)) {
            showCarryOnValue = $showCarryOnFlt.is(':checked');
            showCarryOnFilterElement = showCarryOnValue;
        } else {
            showCarryOnFilterElement = false;
        }
        var wrItmNatureFlt = '';
        if (!($warehouseItemNatureFilter.val() === undefined)) {
            wrItmNatureFlt = $warehouseItemNatureFilter.val();
            productNatureFilterElement = wrItmNatureFlt;
        } else {
            productNatureFilterElement = "";
        }
        var warehouseItemId = 0;
        if (!($warehouseItemId.val() === undefined)) {
            warehouseItemId = $warehouseItemId.val();
            warehouseItemIdFilterElement = warehouseItemId;
        } else {
            warehouseItemIdFilterElement = 0;
        }
        var diaryId = 0;
        if (!($diaryId.val() === undefined)) {
            diaryId = $diaryId.val();
            diaryIdFilterElement = diaryId;
        } else {
            diaryIdFilterElement = 0;
        }
    };
    const commonTableHandlers = [
        {
            selector: "input[name=checkAllRows]",
            event: "change",
            handler: function () {
                var th = $(this).index();
                var isChecked = $(this).prop("checked");
                $("input[name=checkTableRow]").prop("checked", isChecked);
                var selectedRowsCount = countSelectedRows();
                if (selectedRowsCount > 0) {
                    $selectedRowsActionsLink.removeClass("disabled");
                } else {
                    $selectedRowsActionsLink.addClass("disabled");
                }
            }
        },
        {
            selector: "input[name=checkTableRow]",
            event: "click",
            handler: function () {
                //indPgLib.handleSelectedRowsUi();
                handleSelectedRowsUi();
            }
        },

        {
            selector: "[name=SortHeader]",
            event: "click",
            handler: function (event) {
                var button = $(event.delegateTarget);
                var reqSort = button.data("sortkey");
                let reqSortType = button.data("sorttype");
                let iconSortType = "";
                var newSortVal = "";
                //var curSort = indPgLib.getTableCurrentSort();
                var curSort = getTableCurrentSort();
                if (curSort === undefined || curSort === null || curSort.length === 0) {
                    curSort = reqSort + ":desc";
                }
                var curSortAr = curSort.split(":");
                if (curSortAr[0] === reqSort) {
                    var newSort = curSortAr[1] === "asc" ? "desc" : "asc";
                    var newSortIconType = curSortAr[1] === "asc" ? "-down" : "-up";
                    newSortVal = curSortAr[0] + ":" + newSort;
                    iconSortType = "fas fa-sort-" + reqSortType + newSortIconType;
                } else {
                    newSortVal = reqSort + ":asc";
                    iconSortType = "fas fa-sort-" + reqSortType + "-up";
                }
                let $btParent = button.parent();
                let $icon = $btParent.find("i:eq(0)");
                if ($icon !== undefined) {
                    $icon.removeClass();
                    $icon.addClass(iconSortType);
                }
                setTableCurrentSort(newSortVal);
                refreshTableData();
            }
        }
    ];

    const commonHandlers = [

        {
            selector: "#FiltersForm",
            event: "submit",
            handler: function (e) {
                e.preventDefault();
                var $icon = $(".search_icon").find("svg");
                if ($icon.hasClass("fa-search")) {
                    $icon.addClass("fa-times").removeClass("fa-search");
                } else {
                    $icon.addClass("fa-search").removeClass("fa-times");
                }
                //indPgLib.refreshData();
                refreshTableData();
            },
        },
        {
            selector: "#filterCollapse",
            event: "hidden.bs.collapse",
            handler: function (event) {
                $filtersToggle.text("Show Filters");
                $filtersVisible.val(false);
            },
        },
        {
            selector: "#filterCollapse",
            event: "shown.bs.collapse",
            handler: function (event) {
                $filtersToggle.text("Hide Filters");
                $filtersVisible.val(true);
            },
        },
        {
            selector: "#PageSize",
            event: "change",
            handler: function (event) {
                $pageIndex.val(1);
                refreshTableData();
            },
        },
        {
            selector: ".search_icon",
            event: "click",
            handler: function (event) {
                let icon = $(this).find("svg");
                if (icon.hasClass("fa-search")) {
                    icon.addClass("fa-times").removeClass("fa-search");
                } else {
                    icon.addClass("fa-search").removeClass("fa-times");
                    $(".search_input").val("");
                }

                //indPgLib.refreshData();
                refreshTableData();
            },
        },
        {
            selector: "#DatePeriodFilter",
            event: "change",
            handler: function (event) {
                $pageIndex.val(1);
                //indPgLib.refreshData();
                refreshTableData();
            },
        },
        {
            selector: "#CompanyFilter",
            event: "change",
            handler: function (event) {
                $pageIndex.val(1);
                //indPgLib.refreshData();
                refreshTableData();
            },
        },
        {
            selector: "#rowSelectorsToggle",
            event: "click",
            handler: function () {
                rowSelectorsToggleHandler();
            },
        },
    ];

    //Support functions===============================================
    const setupCreateNewElement = () => {
        let el = document.getElementById("CreateNew");
        if (el) {
            let curLocation = window.location.href;
            let srcTerm = "/Create";
            let hRefToAssign = window.location.href;
            if (!curLocation.endsWith(srcTerm)) {
                hRefToAssign=`${window.location.href}/Create`;
            }
            el.href = hRefToAssign;
        }

    };
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
    const getIndexTableCurrentSort = () => {
        try {
            //let cSort = currentTableSort;
            let cSort = $currentSort.val();
            return cSort;
        } catch (e) {
            return "";
        }
    };
    const setIndexTableCurrentSort = (currentSort) => {
        //currentTableSort= currentSort;
        $currentSort.val(currentSort);
    };

    const countSelectedRows = () => {
        const $rowSelectors = $("input[name=checkTableRow]");
        var selectedRows = $rowSelectors.filter(":checked");
        return selectedRows.length;
    };
    const rowSelectorsUi = () => {
        var $selectSingleSelectors = $("td[name=selectRowColumn]");
        var $selectAllSelector = $("th[name=selectAllRowsColumn]");
        var $selectedRowsActionsMenu = $("#SelectedRowsActionsMenu");
        var $selectedRowsActionsLink = $("#ddSelectedRowsActions");
        var $rowSelectorsToggle = $("#rowSelectorsToggle");
        var $rowSelectorsVisible = $("#rowSelectorsVisible");

        var selectorsVisible = $rowSelectorsVisible.val();
        var newSelectorsVisible =
            selectorsVisible == "True" || selectorsVisible == "true" || selectorsVisible === true ? "True" : "False";
        if (newSelectorsVisible == "True") {
            $selectAllSelector.removeAttr("hidden");
            $selectSingleSelectors.removeAttr("hidden");
            $selectAllSelector.show();
            $selectSingleSelectors.show();
            $rowSelectorsVisible.val(true);
            $rowSelectorsToggle.text("Hide Row Selectors");
            $selectedRowsActionsMenu.removeAttr("hidden");
            $selectedRowsActionsLink.addClass("disabled");
        } else {
            $selectAllSelector.hide();
            $selectSingleSelectors.hide();
            $rowSelectorsVisible.val(false);
            $rowSelectorsToggle.text("Show Row Selectors");
            $selectedRowsActionsMenu.attr("hidden", "true");
        }
    };
    const handleSelectedRowsUi = () => {
        var $rowSelectors = $("input[name=checkTableRow]");
        var $checkAllRows = $("input[name=checkAllRows]");
        var $selectedRowsActionsLink = $("#ddSelectedRowsActions");
        var allRows = $rowSelectors.length;
        var selectedRowsCount = $rowSelectors.filter(":checked").length;
        if (selectedRowsCount > 0) {
            $selectedRowsActionsLink.removeClass("disabled");
            //$selectedRowsActionsLink.prop('disabled', 'false');
        } else {
            $selectedRowsActionsLink.addClass("disabled");
        }
        if (selectedRowsCount == allRows) {
            $checkAllRows.prop("checked", true);
        } else {
            $checkAllRows.prop("checked", false);
        }
    };
    const rowSelectorsToggleHandler = () => {
        const $selectSingleSelectors = $("td[name=selectRowColumn]");
        const $selectAllSelector = $("th[name=selectAllRowsColumn]");
        const $rowSelectorsVisible = $("#rowSelectorsVisible");
        const $rowSelectorsToggle = $("#rowSelectorsToggle");
        const $selectedRowsActionsMenu = $("#SelectedRowsActionsMenu");
        const $selectedRowsActionsLink = $("#ddSelectedRowsActions");

        var selectorsVisible = $rowSelectorsVisible.val();
        var newSelectorsVisible =
            selectorsVisible == "True" || selectorsVisible == "true" || selectorsVisible === true ? "False" : "True";
        if (newSelectorsVisible == "True") {
            $selectAllSelector.removeAttr("hidden");
            $selectSingleSelectors.removeAttr("hidden");
            $selectAllSelector.show();
            $selectSingleSelectors.show();
            $rowSelectorsVisible.val(true);
            $rowSelectorsToggle.text("Hide Row Selectors");
            $selectedRowsActionsMenu.removeAttr("hidden");
            $selectedRowsActionsLink.addClass("disabled");
        } else {
            $selectAllSelector.hide();
            $selectSingleSelectors.hide();
            $rowSelectorsVisible.val(false);
            $rowSelectorsToggle.text("Show Row Selectors");
            $selectedRowsActionsMenu.attr("hidden", "true");
        }
    };
    const handleFiltersUi = () => {
        var filterVisible = $filtersVisible.val();
        if (filterVisible === "True" || filterVisible === "true") {
            $filterCollapse.collapse("show");
        } else {
            $filterCollapse.collapse("hide");
        }
    };
    const reallyIsNaN = (x) => {
        return x !== x;
    };
    const isEmpty = (inputObject) => {
        return Object.keys(inputObject).length === 0;
    };
    //================================================================
    const getConditionValue = (itemType, itemValue, valueSource) => {
        let returnValue = '';

        switch (itemType) {
            case 'key':
                returnValue = valueSource[itemValue];
                break;
            case 'func':
                returnValue = itemValue(valueSource);
                break;
            default:
        }
        return returnValue;
    };
    const evaluateCondition = (cond, valueSource) => {
        let evaluationResult = true;
        let val1 = getConditionValue(cond.val1Type, cond.val1Value, valueSource);
        let val2 = getConditionValue(cond.val2Type, cond.val2Value, valueSource);
        if (cond.operator === 'notZeroDiff') {
            let diffAmount = parseFloat(val1) - parseFloat(val2);
            if (diffAmount !== 0) {
                evaluationResult = true;
            } else {
                evaluationResult = false;
            }
        }
        if (cond.operator === 'isGraterThan') {
            if (val1 > val2) {
                evaluationResult = true;
            } else {
                evaluationResult = false;
            }

        }
        return evaluationResult;
    };
    const registerHandlers = (handlerDefinitions) => {
        handlerDefinitions.forEach(function (item) {
            $(item.selector).on(item.event, item.handler);
        });
    };
    const createHeaderColumn = (item) => {
        let curSortUndefined = false;
        let curSortAr = [];
        var curSort = getTableCurrentSort();
        if (curSort === undefined || curSort === null || curSort.length === 0) {
            curSortUndefined = true;
        } else {
            curSortAr = curSort.split(":");
        }
        var tdColHead = "";
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
                    var newSortIconType = curSortAr[1] === "asc" ? "-down" : "-up";
                    var iconSortType = "fas fa-sort-" + item.sortType + newSortIconType;
                    colHtml += `<i class="${iconSortType}" name="SortIcon"></i>  `;
                } else {
                    colHtml += `<i class="" name="SortIcon"></i>  `;
                }
            }

            colHtml += `</a>  `;
            tdColHead = colHtml;
        } else {
            tdColHead = $("<th>").text(item.header).addClass(item.headerClass);
        }
        return tdColHead;
    };
    const createValueColumn = (col, value) => {
        let $tdCol = $("<td>");
        if (col.hasOwnProperty('colType')) {
            if (col.colType === 'imageViewer') {
                let vrLink = '<a href = "#" role = "button" data-bs-toggle="modal"';
                vrLink += ` data-bs-target="${col.imageViewer.target}"`;
                col.imageViewer.dataAttributes.forEach(function (item) {
                    vrLink += ` data-${item.key}="${value[item.valueKey]}"`;
                });
                vrLink += ">";
                vrLink += `<img src="${value[col.key]}" style="${col.imageViewer.style}" />`;
                vrLink += "</a>";
                $tdCol.html(vrLink);
                return $tdCol;
            }
        }
        if (col.responseKey) {
            if (!isEmpty(col.remoteReference)) {
                let valueKey = col.remoteReference.valueKey;
                var remoteId = value[valueKey];
                let remoteUrl = `${col.remoteReference.url}${remoteId}`;
                var $aLink = `<a href="${remoteUrl}" target="_blank">${value[col.responseKey]}</a>  `;
                //$tdCol.innerhtml ($aLink);
                //$($aLink).appendTo($tdCol);
                $tdCol.html($aLink);
            } else {
                switch (col.columnFormat) {
                    case "t":
                        $tdCol.text(value[col.responseKey]);
                        break;
                    case "d":
                        $tdCol.text(moment(value[col.responseKey]).format("DD/MM/YYYY"));
                        break;
                    case "c":
                        $tdCol.text(currencyFormatter.format(value[col.responseKey]));
                        $tdCol.attr("data-actualValue", value[col.responseKey]);
                        break;
                    case "n":
                        $tdCol.text(numberFormatter.format(value[col.responseKey]));
                        $tdCol.attr("data-actualValue", value[col.responseKey]);
                        break;
                    default:
                }
            }
        } else {
            switch (col.columnFormat) {
                case "t":
                    break;
                case "d":
                    break;
                case "c":
                    $tdCol.attr("data-actualValue", 0);
                    break;
                default:
            }
        }

        if (isEmpty(col.classCondition)) {
            $tdCol.addClass(col.class);
        } else {
            if (evaluateCondition(col.classCondition, value)) {
                $tdCol.addClass(col.classWhenCondition);
            } else {
                $tdCol.addClass(col.class);
            }
        }

        return $tdCol;
    };
    const createActionHtml = (col, value, actionSection) => {
        let actionHtml = "";
        let classText = "";

        if (actionSection !== undefined) {
            switch (actionSection) {
                case "sectionActions":
                    classText = `class="me-1"`;
                    break;
                case "sectionSubActions":
                    classText = `class="dropdown-item"`;
                    break
                default:
                    classText = "";

            }
        }
        switch (col.actionType) {
            case "defaultAction2":
                switch (col.uriType) {
                    case "funcHref":
                        let u = col.uriFunc(col, value);
                        actionHtml += `<a ${classText} href="${u}" target="${col.target}">`;
                        break;
                    case "funcUrl":
                        break;
                    case "staticHref":
                        var params = '';
                        for (var i = 0; i < col.urlKeys.length - 1; i++) {
                            if (i > 0) {
                                params += "&";
                            } else {
                                params += "?";
                            }
                            params += `${col.urlKeys[i].key}=${col.urlKeys[i].key}`;
                        }

                        actionHtml += `<a ${classText} href="${col.uri}${params}" target="${col.target}">`;
                        break;
                    case "staticUrl":
                        break;
                    default:
                }
                break;
            case "defaultAction":
                actionHtml += `<a ${classText} href="${col.url}${value[col.valueKey]}">`;
                break;
            case "newWindowAction":
                actionHtml += `<a ${classText} target="_blanc" href="${col.url}${value[col.valueKey]}">`;
                break;
            case "eventAction":
                actionHtml += `<a ${classText} href="#"`;
                if (col.elementName) {
                    actionHtml += `name=${col.elementName}`;
                }

                actionHtml += ` data-${col.dataKey}=${value[col.valueKey]}`;
                actionHtml += ">";
                break;
            case "modalSelectorEventAction":
                actionHtml += `<a ${classText} href="#" data-bs-toggle="modal"`;
                if (col.elementName) {
                    actionHtml += ` name=${col.elementName}`;
                }
                if (col.selectorTarget) {
                    actionHtml += ` data-bs-target=${col.selectorTarget}`;
                }
                if (col.selectorType) {
                    actionHtml += ` data-selectorType=${col.selectorType}`;
                }
                actionHtml += ` data-docid=${value[col.valueKey]}`;
                actionHtml += ">";
                break;
            default:
        }
        if (!(col.icon === undefined)) {
            actionHtml += col.icon;
        }
        if (!(col.text === undefined)) {
            if (actionSection !== undefined) {
                if (actionSection === "sectionSubActions") {
                    actionHtml += col.text;
                }
            }

        }
        actionHtml += "</a>";
        return actionHtml;
    };
    const createColumnAction = (col, value) => {
       
        return createActionHtml(col, value, "sectionActions");

    };
    const createColumnSubAction = (col, value) => {
        let actionHtml = '';
        let visibility = true;
        if (col.visibility === "condition") {
            visibility = false;
            if (!isEmpty(col.condition)) {
                if (evaluateCondition(col.condition, value)) {
                    visibility = true;
                }

            }
        }
        if (visibility) {
            actionHtml = createActionHtml(col, value, "sectionSubActions");
        }
        return actionHtml;
    };
    const createMobileColumnAction = (col, value) => {
        let actionHtml = "";
        let visibility = true;
        if (col.visibility === "condition") {
            visibility = false;
            if (!isEmpty(col.condition)) {
                if (evaluateCondition(col.condition, value)) {
                    visibility = true;
                }

            }
        }
        if (visibility) {
            actionHtml = createActionHtml(col, value);
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
        tableHandlersToRegister = pgDefinition.tableHandlersToRegister;
        pageHandlersToRegister = pgDefinition.pageHandlersToRegister;
        actionMobileColDefs = pgDefinition.actionMobileColDefs;
        tableElementId = pgDefinition.tableElementId === undefined ? "myTable" : pgDefinition.tableElementId;
        indexPageType = pgDefinition.indexPageType === undefined ? definitionsLib.IndexPageTypeEnum.IndexPage : pgDefinition.indexPageType;
        if (indexPageType === definitionsLib.IndexPageTypeEnum.IndexPage) {
            spinnerLoaderShow = indexPageSpinnerShow;
            spinnerLoaderHide = indexPageSpinnerHide;
            spinnerLoaderIsVisible = indexPageSpinnerIsVisible;
            getTableCurrentSort = getIndexTableCurrentSort;
            setTableCurrentSort = setIndexTableCurrentSort;
            setFilterValues = setIndexPageFilterValues;
        } else {
            spinnerLoaderShow = selectorSpinnerShow;
            spinnerLoaderHide = selectorSpinnerHide;
            spinnerLoaderIsVisible = selectorSpinnerIsVisible;
            getTableCurrentSort = pgDefinition.getTableCurrentSort;
            setTableCurrentSort = pgDefinition.setTableCurrentSort;
            setFilterValues = setSelectorFilterValues;
        }
    };
    const setCurrencyFormatter = (formatter) => {
        currencyFormatter = formatter;
    };
    const setNumberFormatter = (formatter) => {
        numberFormatter = formatter;
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
    const getTableData = function (pgIndex, pgSize, sortData, dateRange
                                   , companyFlt, searchFlt, currencyFlt
                                   , transTypeFlt, wrItmNatureFlt, transactorId
                                   ,warehouseItemId,diaryId,cfaId,showCarryOnFlt) {
        let uri = indexPageDefinition.uri;
        uri += `?pageIndex=${pgIndex}`;
        uri += `&pageSize=${pgSize}`;
        uri += `&companyFilter=${companyFlt}`;
        uri += `&dateRange=${dateRange}`;
        uri += `&sortData=${sortData}`;
        uri += `&searchFilter=${searchFlt}`;
        uri += `&transactorTypeFilter=${transTypeFlt}`;
        uri += `&warehouseItemNatureFilter=${wrItmNatureFlt}`;
        uri += `&transactorId=${transactorId}`;
        uri += `&cashFlowAccountId=${cfaId}`;
        uri += `&showCarryOnAmountsInTabs=${showCarryOnFlt}`;
        uri += `&warehouseItemId=${warehouseItemId}`;
        uri += `&diaryId=${diaryId}`;
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
                        spinnerLoaderShow();
                    }, 1000);
                },
                complete: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    $("#loadMe").modal("hide");
                    setTimeout(function () {
                        var isOpen = spinnerLoaderIsVisible();
                        if (isOpen) {
                            spinnerLoaderHide();
                        }
                    }, 2000);
                },
            });
        });
    };
    const bindDataToTable = (result, pgIndex) => {
        handlePagingUi(result.totalPages, result.totalRecords, pgIndex, result.hasPrevious, result.hasNext);


        $(`#${tableElementId} > tbody`).empty();
        $(`#${tableElementId} > thead`).empty();

        var $trHead = $("<tr>");
        if (indexPageType === definitionsLib.IndexPageTypeEnum.IndexPage) {
            var $tdSelectCol = $(selectAllRowsColumnHtml());
            $trHead.append($tdSelectCol);
        }

        colDefs.forEach(function (item) {
            $trHead.append($(createHeaderColumn(item)));
        });
        $trHead.append($("<th>"));
        $trHead.appendTo(`#${tableElementId} > thead`);

        $.each(result.data, function (index, value) {
            var itemId = value.id;
            var $tr = $("<tr>");
            if (indexPageType === definitionsLib.IndexPageTypeEnum.IndexPage) {
                var $tdSelectRowCol = $(selectOneRowColumnHtml(itemId));
                $tr.append($tdSelectRowCol);
            }


            colDefs.forEach(function (col) {
                $tr.append(createValueColumn(col, value));
            });

            let actionHtml = "";
            actionColDefs.forEach(function (col) {
                actionHtml += createColumnAction(col, value);
            });
            if (actionColSubDefs.length > 0) {
                actionHtml += '<a class="dropdown-toggle" role="button" id="dropdownMenuButton"';
                actionHtml += 'data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                actionHtml += '<i class="fas fa-bars" style="color:slategray"></i></a>';
                actionHtml += '<div class="dropdown-menu small" aria-labelledby="dropdownMenuButton">';

                actionColSubDefs.forEach(function (col) {
                    actionHtml += createColumnSubAction(col, value);
                });
                actionHtml += "</div>";
            }
            let mobileHtml = "";
            if (actionMobileColDefs.length > 0) {
                mobileHtml += '<a class="dropdown-toggle" role="button" id="dropdownMenuButton"';
                mobileHtml += 'data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">';
                mobileHtml += '<i class="fas fa-bars" style="color:slategray"></i></a>';
                mobileHtml += '<div class="dropdown-menu small" aria-labelledby="dropdownMenuButton">';

                actionMobileColDefs.forEach(function (col) {
                    mobileHtml += createMobileColumnAction(col, value);
                });
                mobileHtml += "</div>";
            }
            var actionsCol = `<td class="small text-center d-none d-lg-table-cell">${actionHtml}</td>`;
            var mobileCol = `<td class="small text-center d-table-cell d-sm-table-cell d-md-table-cell d-lg-none">${mobileHtml}</td>`;
            $tr.append(actionsCol);
            $tr.append(mobileCol);
            $tr.appendTo(`#${tableElementId} > tbody`);
        });
        registerHandlers(commonTableHandlers);
        registerHandlers(tableHandlersToRegister);
        //====================
        let pageSummaryCount = 0;
        let totalSummaryCount = 0;
        let $pageSummaryRow = $('<tr class="table-info">');
        $pageSummaryRow.append('<td name="selectRowColumn"> </td> ');
        let $totalSummaryRow = $('<tr class="table-info">');
        if (indexPageType === definitionsLib.IndexPageTypeEnum.IndexPage) {
            $totalSummaryRow.append('<td name="selectRowColumn"> </td> ');
        }
        colDefs.forEach(function (item) {
            var tdColPage = "";
            var tdColTotal = "";
            if (item.totalKey.length !== 0) {
                if (item.totalKey === "label") {
                    tdColPage = $("<td>").text("Σύνολα Σελίδας").addClass("small font-weight-bold");
                } else {
                    let tmpAmount = 0;
                    switch (item.totalFormatter) {
                        case "currency":
                            tmpAmount = currencyFormatter.format(result[item.totalKey]);
                            break;
                        case "number":
                            tmpAmount = numberFormatter.format(result[item.totalKey]);
                            break;
                        default:
                            tmpAmount = numberFormatter.format(result[item.totalKey]);
                            break;
                    }
                    try {

                        tdColPage = `<td class="${item.class} font-weight-bold"> `;
                        tdColPage += `${tmpAmount} </td> `;
                        pageSummaryCount++;
                    } catch (e) {
                        tdColPage = `<td class="${item.class}"> `;
                        tdColPage += `#Err </td> `;
                    }

                }
            } else {
                tdColPage = `<td class="${item.class}"> `;
                tdColPage += ` </td> `;
            }
            if (item.grandTotalKey.length !== 0) {
                if (item.grandTotalKey === "label") {
                    tdColTotal = $("<td>").text("Γενικό Σύνολο").addClass("small font-weight-bold");
                } else {
                    let tmpAmount = 0;
                    switch (item.totalFormatter) {
                        case "currency":
                            tmpAmount = currencyFormatter.format(result[item.grandTotalKey]);
                            break;
                        case "number":
                            tmpAmount = numberFormatter.format(result[item.grandTotalKey]);
                            break;
                        default:
                            tmpAmount = numberFormatter.format(result[item.grandTotalKey]);
                            break;
                    }
                    try {

                        tdColTotal = `<td class="${item.class} font-weight-bold"> `;
                        tdColTotal += `${tmpAmount} </td> `;
                        totalSummaryCount++;
                    } catch (e) {
                        tdColTotal = `<td class="${item.class}"> `;
                        tdColTotal += `#Err </td> `;
                    }

                }
            } else {
                tdColTotal = `<td class="${item.class}"> `;
                tdColTotal += ` </td> `;
            }
            $pageSummaryRow.append($(tdColPage));
            $totalSummaryRow.append($(tdColTotal));
        });
        if (pageSummaryCount > 0) {
            $pageSummaryRow.append($("<td>"));
            $pageSummaryRow.appendTo(`#${tableElementId} >  tbody:last`);
        }
        if (totalSummaryCount > 0) {
            $totalSummaryRow.append($("<td>"));
            $totalSummaryRow.appendTo(`#${tableElementId} >  tbody:last`);
        }
        if (!isEmpty(indexPageDefinition.afterTableLoad)) {
            indexPageDefinition.afterTableLoad.callback(result);
        }
        rowSelectorsUi();
    };
  
    const refreshTableData = () => {
        setFilterValues();
        getTableData(pageIndexElement, pageSizeElement, tableCurrentSortElement
            , datePeriodFilterElement, companyFilterElement, searchTextElement
            , currencyFilterElement, transactorTypeFilterElement
            , productNatureFilterElement, transactorIdFilterElement
            , warehouseItemIdFilterElement, diaryIdFilterElement
            , cfaIdFilterElement,showCarryOnFilterElement)
            .then((data) => {
                bindDataToTable(data, pageIndexElement);
            })
            .catch((error) => {
                console.log(error);
            });
    };

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
                        refreshTableData();
                        break;
                    case "GoToPrevious":
                        curPageIndex = parseInt($pageIndex.val());
                        curPageIndex -= 1;
                        $pageIndex.val(curPageIndex);
                        refreshTableData();
                        break;
                    case "GoToNext":
                        curPageIndex = parseInt($pageIndex.val());
                        curPageIndex += 1;
                        $pageIndex.val(curPageIndex);
                        refreshTableData();
                        break;
                    case "GoToLast":
                        curPageIndex = parseInt($totalPages.val());
                        $pageIndex.val(curPageIndex);
                        refreshTableData();
                        break;
                    default:
                }
            });
        });
    };
    const registerPageHandlers = () => {
        registerHandlers(pageHandlersToRegister);
    };
    const loadSettings = (localStorageKey) => {
        var storageItemJs = localStorage.getItem(localStorageKey);
        if (storageItemJs === undefined || storageItemJs === null) {
            $pageIndex.val(1);
            $("#filtersVisible").val(true);
            $("#rowSelectorsVisible").val(true);
            // $rowSelectorsToggle.text('Hide Row Selectors');
            // $filtersToggle.text('Hide Filters');
            $datePeriodFilter.val("CURMONTH");
            $companyFilter.val(0);
            $currentSort.val("transactiondate:desc");
        }
        else {
            var storageItem = JSON.parse(storageItemJs);
            var filtersValue = storageItem.find((x) => x.filterKey === "filtersVisible").filterValue;
            if (filtersValue === true) {
                $("#filtersToggle").text("Hide Filters");
            } else {
                $("#filtersToggle").text("Show Filters");
            }
            $("#filtersVisible").val(filtersValue);
            filtersValue = storageItem.find((x) => x.filterKey === "rowSelectorsVisible").filterValue;
            $("#rowSelectorsVisible").val(filtersValue);

            filtersValue = storageItem.find((x) => x.filterKey === "dateRangeFilter").filterValue;
            $datePeriodFilter.val(filtersValue);
            filtersValue = storageItem.find((x) => x.filterKey === "currentSort").filterValue;
            $currentSort.val(filtersValue);
            filtersValue = storageItem.find((x) => x.filterKey === "companyFilter").filterValue;
            $companyFilter.val(filtersValue);
            filtersValue = storageItem.find((x) => x.filterKey === "pageSize").filterValue;
            $pageSize.val(filtersValue);
            filtersValue = storageItem.find((x) => x.filterKey === "pageIndex").filterValue;
            $pageIndex.val(filtersValue);

            try {
                filtersValue = storageItem.find((x) => x.filterKey === "currentCurrency").filterValue;
                $currencySelector.val(filtersValue);
            } catch (e) { }
        }
    };
    const saveSettings = (localStorageKey) => {
        //#region CommentOut
        //#region Variables
        var filtersVisible = $("#filterCollapse").hasClass("show");
        var $rowSelectorsColumn = $("th[name=selectAllRowsColumn]");

        var rowSelectorsNotVisible = $rowSelectorsColumn.is(":hidden");
        var rowSelectorsVisible = rowSelectorsNotVisible ? false : true;

        var pageSize = $pageSize.val();
        var dateRange = $datePeriodFilter.val();
        var currentSort = $currentSort.val();
        var companyFilter = $companyFilter.val();
        var pageIndex = $pageIndex.val();
        var currentCurrency = $currencySelector.val();
        var filtersArr = [];
        //#endregion
        filtersArr.push({
            filterKey: "filtersVisible",
            filterValue: filtersVisible,
        });
        filtersArr.push({
            filterKey: "rowSelectorsVisible",
            filterValue: rowSelectorsVisible,
        });
        filtersArr.push({
            filterKey: "pageIndex",
            filterValue: pageIndex,
        });
        filtersArr.push({
            filterKey: "pageSize",
            filterValue: pageSize,
        });
        filtersArr.push({
            filterKey: "dateRangeFilter",
            filterValue: dateRange,
        });
        filtersArr.push({
            filterKey: "currentSort",
            filterValue: currentSort,
        });
        filtersArr.push({
            filterKey: "companyFilter",
            filterValue: companyFilter,
        });
        filtersArr.push({
            filterKey: "currentCurrency",
            filterValue: currentCurrency,
        });

        var sessionVal = JSON.stringify(filtersArr);

        localStorage.setItem(localStorageKey, sessionVal);
        //#endregion
    };
    //register common handlers
    setupCreateNewElement();
    registerHandlers(commonHandlers);
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
        registerPageHandlers: registerPageHandlers,
        handleFiltersUi: handleFiltersUi,
        setCurrencyFormatter: setCurrencyFormatter,
        setNumberFormatter: setNumberFormatter,
        loadSettings: loadSettings,
        saveSettings: saveSettings
        
    };
})();
