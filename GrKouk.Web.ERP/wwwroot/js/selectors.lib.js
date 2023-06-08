//Author: George Koukoudis
//Version:  1.0.0.1
//Date Created: 
//Date Modified: 05/06/2023
//This library is used in selectors inside pages (transactors or warehouseitems)
var selectorsLib = (function () {
    const $tblElement=$('#selectorItemsList');
    const $tblHeader = $('#selectorItemsList > thead');
    const $tblBody = $('#selectorItemsList > tbody');
    const $searchTextElement = $('[name=itemsSearchText]');
    let formatterCurrency = '';
    let formatterNumber = '';
    let selectorDef;
    let seekItems;
    let itemSelected;
    //Spinner loader stuff
    const spinnerLoaderShow = ($spinnerElement) => {
        $spinnerElement.show();
    };
    const spinnerLoaderHide = ($spinnerElement) => {
        $spinnerElement.hide();
    };
    const spinnerLoaderIsVisible = ($spinnerElement) => {
        return($spinnerElement.is(':visible'));
    };
    var timeKey;
    let transactorTypesSelected="";
    let companiesSelected="";
    let productNaturesSelected="";
    let searchText = "";

    //Sort Data Management
    const getProductSelectorSortData = () => {
        var storageItemJs = localStorage.getItem("ProductSelectorSortData");
        if (storageItemJs === undefined || storageItemJs === null) {
            return "";
        } 
        else {
            var storageItem = JSON.parse(storageItemJs);
            return storageItem;
        }
    };
    const getSupplierSelectorSortData = () => {
        var storageItemJs = localStorage.getItem("SupplierSelectorSortData");
        if (storageItemJs === undefined || storageItemJs === null) {
            return "";
        } 
        else {
            var storageItem = JSON.parse(storageItemJs);
            return storageItem;
        }
    };
    const getCustomerSelectorSortData = () => {
        var storageItemJs = localStorage.getItem("CustomerSelectorSortData");
        if (storageItemJs === undefined || storageItemJs === null) {
            return "";
        } 
        else {
            var storageItem = JSON.parse(storageItemJs);
            return storageItem;
        }
    };
   
    const setProductSelectorSortData = (sortData) => {
        var sessionVal = JSON.stringify(sortData);
        localStorage.setItem("ProductSelectorSortData", sessionVal);
    };
    const setSupplierSelectorSortData = (sortData) => {
        var sessionVal = JSON.stringify(sortData);
        localStorage.setItem("SupplierSelectorSortData", sessionVal);
    };
    const setCustomerSelectorSortData = (sortData) => {
        var sessionVal = JSON.stringify(sortData);
        localStorage.setItem("CustomerSelectorSortData", sessionVal);
    };

    const getFilterValues = () => {
        return {
            companyFilterElement : companiesSelected,
            datePeriodFilterElement : "",
            pageIndexElement : 1,
            pageSizeElement : 200,
            searchTextElement : searchText,
            currencyFilterElement : 1,
            transactorTypeFilterElement :transactorTypesSelected,
            transactorIdFilterElement : 0,
            cfaIdFilterElement:0,
            warehouseItemIdFilterElement:0,
            productNatureFilterElement : productNaturesSelected,
            diaryIdFilterElement:0,
            showCarryOnFilterElement:false,
            showSummaryFilterElement:false,
            showDisplayLinesWithZeroesFilterElement:false
        };
    };
    const colDefsTransactors = [
        {
            key: 'code',
            responseKey: 'code',
            actualVal: '',
            columnFormat: 't',
            totalKey: '',
            grandTotalKey: '',
            totalFormatter: '',
            sortKey: 'TransactorCodeSort',
            sortType: 'alpha',
            header: 'Code',
            text: '',
            headerClass: 'small text-center',
            class: 'small',
            classWhenCondition: '',
            classCondition: {},
            remoteReference: {
            }
        },
        {
            key: 'name',
            responseKey: 'name',
            actualVal: '',
            columnFormat: 't',
            totalKey: '',
            grandTotalKey: '',
            totalFormatter: '',
            sortKey: 'TransactorNameSort',
            sortType: 'alpha',
            header: 'Transactor',
            text: '',
            headerClass: 'small text-center',
            class: 'small',
            classWhenCondition: '',
            classCondition: {},
            remoteReference: {
            }
        },
        {
            key: 'email',
            responseKey: 'email',
            actualVal: '',
            columnFormat: 't',
            totalKey: '',
            grandTotalKey: '',
            totalFormatter: '',
            sortKey: '',
            sortType: '',
            header: 'email',
            text: '',
            headerClass: 'small text-center d-none d-md-table-cell',
            class: 'small d-none d-md-table-cell',
            classWhenCondition: '',
            classCondition: {},
            remoteReference: {}
        },
        {
            key: 'transactorTypeCode',
            responseKey: 'transactorTypeCode',
            actualVal: '',
            columnFormat: 't',
            totalKey: '',
            grandTotalKey: '',
            totalFormatter: '',
            sortKey: '',
            sortType: '',
            header: 'Type',
            text: '',
            headerClass: 'small text-center',
            class: 'small',
            classWhenCondition: '',
            classCondition: {},
            remoteReference: {}
        },
        {
            key: 'companyCode',
            responseKey: 'companyCode',
            actualVal: '',
            columnFormat: 't',
            totalKey: '',
            grandTotalKey: '',
            totalFormatter: '',
            sortKey: 'CompanyCodeSort',
            sortType: 'alpha',
            header: 'Company',
            text: '',
            headerClass: 'small text-center',
            class: 'small ',
            classWhenCondition: '',
            classCondition: {},
            remoteReference: {
            }
        }
    ];
    const colDefsMaterials = [
        {
            colType: 'imageViewer',
            key: 'url',
            responseKey: 'url',
            actualVal: '',
            columnFormat: 't',
            totalKey: '',
            grandTotalKey: '',
            totalFormatter: '',
            sortKey: '',
            sortType: '',
            header: 'Image',
            text: '',
            headerClass: 'small text-center d-none d-md-table-cell',
            class: 'small d-none d-md-table-cell ',
            classWhenCondition: '',
            classCondition: {},
            remoteReference: {
            },
            imageViewer: {
                target: '#imageViewer',
                style: 'height:50px;width:50px;',
                dataAttributes: [
                    {
                        key: 'imageproductcode',
                        valueKey: 'code'
                    },
                    {
                        key: 'imageproductname',
                        valueKey: 'name'
                    },
                    {
                        key: 'imageurl',
                        valueKey: 'url'
                    }
                ]
            }
        },
        {
            key: 'code',
            responseKey: 'code',
            actualVal: '',
            columnFormat: 't',
            totalKey: '',
            grandTotalKey: '',
            totalFormatter: '',
            sortKey: 'codesort',
            sortType: 'alpha',
            header: 'Code',
            text: '',
            headerClass: 'small text-center d-none d-md-table-cell',
            class: 'small d-none d-md-table-cell ',
            classWhenCondition: '',
            classCondition: {},
            remoteReference: {
            }
        },
        {
            key: 'name',
            responseKey: 'name',
            actualVal: '',
            columnFormat: 't',
            totalKey: 'label',
            grandTotalKey: 'label',
            totalFormatter: '',
            sortKey: 'namesort',
            sortType: 'alpha',
            header: 'Name',
            text: '',
            headerClass: 'small text-center',
            class: 'small',
            classWhenCondition: '',
            classCondition: {},
            remoteReference: {
               
            }
        },


        {
            key: 'warehouseItemNatureName',
            responseKey: 'warehouseItemNatureName',
            actualVal: '',
            columnFormat: 't',
            totalKey: '',
            grandTotalKey: '',
            totalFormatter: '',
            sortKey: '',
            sortType: '',
            header: 'Nature',
            text: '',
            headerClass: 'small text-center d-none d-md-table-cell',
            class: 'small d-none d-md-table-cell ',
            classWhenCondition: '',
            classCondition: {},
            remoteReference: {
            }
        },
        
        {
            key: 'companyCode',
            responseKey: 'companyCode',
            actualVal: '',
            columnFormat: 't',
            totalKey: '',
            grandTotalKey: '',
            totalFormatter: '',
            sortKey: 'CompanyCodeSort',
            sortType: 'alpha',
            header: 'Company',
            text: '',
            headerClass: 'small text-center',
            class: 'small ',
            classWhenCondition: '',
            classCondition: {},
            remoteReference: {
            }
        },
        {
            key: 'active',
            responseKey: 'active',
            actualVal: '',
            columnFormat: 't',
            totalKey: '',
            grandTotalKey: '',
            totalFormatter: '',
            sortKey: '',
            sortType: '',
            header: 'Active',
            text: '',
            headerClass: 'small text-center',
            class: 'small',
            classWhenCondition: '',
            classCondition: {},
            remoteReference: {
            }
        }
    ];
    const pageHandlersToRegister = [
      
    ];
    const actionColDefs = [
        {
            actionType: 'eventAction',
            visibility: 'condition',
            elementName: 'selectItem',
            condition: {
               
            },
            text: '',
            valueKey: 'id',
            dataKey:'itemId',
            icon: '<i class="far fa-check-square fa-lg" style="color:slategray"></i>',
            url: ''
        }
    ];
    const actionColSubDefs = [];
    const actionMobileColDefs = [];
    const tableHandlersToRegister = [
        {
            selector:'[name=selectItem]',
            event:'click',
            handler: function(event) {
                let button = $(event.delegateTarget);
                let itemId = button.data('itemid');
                itemSelected(itemId);
                $('#itemSelector').modal('hide');
            }
        }
    ];
    const supplierSelectorDef = {
        uri : '/api/GrKoukInfoApi/GetSelectorTransactorsV2',
        //uri: '/api/GrKoukInfoApi/GetIndexTblDataTransactors',
        currencyFormatter: formatterCurrency,
        numberFormatter: formatterNumber,
        colDefs: colDefsTransactors,
        actionColDefs: actionColDefs,
        actionColSubDefs: actionColSubDefs,
        actionMobileColDefs: actionMobileColDefs,
        tableHandlersToRegister: tableHandlersToRegister,
        pageHandlersToRegister: pageHandlersToRegister,
        tableElementId:"selectorItemsList",
        indexPageType:definitionsLib.IndexPageTypeEnum.SupplierSelector,
        afterTableLoad: {
           
        },
        getTableCurrentSort: getSupplierSelectorSortData,
        setTableCurrentSort: setSupplierSelectorSortData,
        getFilterValues : getFilterValues
    };
    const customerSelectorDef = {
        uri : '/api/GrKoukInfoApi/GetSelectorTransactorsV2',
        //uri: '/api/GrKoukInfoApi/GetIndexTblDataTransactors',
        currencyFormatter: formatterCurrency,
        numberFormatter: formatterNumber,
        colDefs: colDefsTransactors,
        actionColDefs: actionColDefs,
        actionColSubDefs: actionColSubDefs,
        actionMobileColDefs: actionMobileColDefs,
        tableHandlersToRegister: tableHandlersToRegister,
        pageHandlersToRegister: pageHandlersToRegister,
        tableElementId:"selectorItemsList",
        indexPageType:definitionsLib.IndexPageTypeEnum.CustomerSelector,
        afterTableLoad: {
           
        },
        getTableCurrentSort: getCustomerSelectorSortData,
        setTableCurrentSort: setCustomerSelectorSortData,
        getFilterValues : getFilterValues
    };
    const transactorSelectorDef = {
        uri: '/api/GrKoukInfoApi/GetIndexTblDataTransactors',
        currencyFormatter: formatterCurrency,
        numberFormatter: formatterNumber,
        colDefs: colDefsTransactors,
        actionColDefs: actionColDefs,
        actionColSubDefs: actionColSubDefs,
        actionMobileColDefs: actionMobileColDefs,
        tableHandlersToRegister: tableHandlersToRegister,
        pageHandlersToRegister: pageHandlersToRegister,
        tableElementId:"selectorItemsList",
        indexPageType:definitionsLib.IndexPageTypeEnum.TransactorSelector,
        afterTableLoad: {
           
        }
    };
    const materialSelectorDef = {
        uri: '/api/GrKoukInfoApi/GetIndexTblDataWarehouseItemsV2',
        currencyFormatter: formatterCurrency,
        numberFormatter: formatterNumber,
        colDefs: colDefsMaterials,
        actionColDefs: actionColDefs,
        actionColSubDefs: actionColSubDefs,
        actionMobileColDefs: actionMobileColDefs,
        tableHandlersToRegister: tableHandlersToRegister,
        pageHandlersToRegister: pageHandlersToRegister,
        tableElementId:"selectorItemsList",
        indexPageType:definitionsLib.IndexPageTypeEnum.ProductSelector,
        afterTableLoad: {},
        getTableCurrentSort: getProductSelectorSortData,
        setTableCurrentSort: setProductSelectorSortData,
        getFilterValues : getFilterValues
    };
   //---------------------------------------
   const clearTable = () => {
       $tblBody.empty();
       $tblHeader.empty();
   };
   
   const seekSuppliers = () => {
       indPgLib.refreshData();
   };
   const seekCustomers = () => {
       indPgLib.refreshData();
   };

   const seekProducts = () => {
       indPgLib.refreshData();
   };
  

   const transactorSelected = (transactorId) => {
       $('#ItemVm_TransactorId').val(transactorId);
   };
   const productSelected = (productId) => {
       getWarehouseItemInfo(productId);
   };
   //---------------------------------------
  
   
   const makeAjaxCall = (uri,$SpElement) => {
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
                        spinnerLoaderShow($SpElement);
                    }, 1000);
                },
                complete: function () {
                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    $("#loadMe").modal("hide");
                    setTimeout(function () {
                        var isOpen = spinnerLoaderIsVisible($SpElement);
                        if (isOpen) {
                            spinnerLoaderHide($SpElement);
                        }
                    }, 2000);
                },
            });
        });
    };
    const getWarehouseItemNatureList = () => {
        let uri = "/api/GrKoukInfoApi/GetSelectorMaterialNatures";

        makeAjaxCall(uri,$("#SpinnerLoaderMaterialNatures"))
            .then((data) => {
                var multiSelect = document.getElementById("MaterialNatureTypes").ej2_instances[0];
                multiSelect.dataSource = data;
                multiSelect.changeOnBlur=false;
                multiSelect.addEventListener("change",(event)=>{
              
                    var selected = JSON.stringify(event.value);
                    productNaturesSelected=selected;
                });
                multiSelect.addEventListener("close",(event)=>{
                    indPgLib.refreshData();
                });
            })
            .catch((error) => {
                console.log(error);
            });
       
    };
    const getTransactorTypeList = () => {
        var uri = '/api/GrKoukInfoApi/GetSelectorTransactorTypes';
        makeAjaxCall(uri,$("#SpinnerLoaderTransactorTypes"))
            .then((data) => {
                var multiSelect = document.getElementById("TransactorTypes").ej2_instances[0];
                multiSelect.dataSource = data;
                multiSelect.changeOnBlur=false;
                multiSelect.addEventListener("change",(event)=>{
              
                    var selected = JSON.stringify(event.value);
                    transactorTypesSelected=selected;
                   
                });
                multiSelect.addEventListener("close",(event)=>{
                    indPgLib.refreshData();
                });
            })
            .catch((error) => {
                console.log(error);
            });
    };
   
    const getCompanyList = () => {
        var uri = '/api/GrKoukInfoApi/GetSelectorCompanies';
        makeAjaxCall(uri,$("#SpinnerLoaderCompanies"))
            .then((data) => {
                var multiSelect = document.getElementById("CompaniesList").ej2_instances[0];
                multiSelect.dataSource = data;
                multiSelect.changeOnBlur=false;
                multiSelect.addEventListener("change",(event)=>{
                    var selected = JSON.stringify(event.value);
                    companiesSelected=selected;
                    
                });
                multiSelect.addEventListener("close",(event)=>{
                    indPgLib.refreshData();
                });

            })
            .catch((error) => {
                console.log(error);
            });
    };
    const updateFilterControls = () => {
        var trTypes = document.getElementById("TransactorTypes").ej2_instances[0];
        var natures = document.getElementById("MaterialNatureTypes").ej2_instances[0];
        var companies = document.getElementById("CompaniesList").ej2_instances[0];
        if (transactorTypesSelected) {
            let sl = JSON.parse(transactorTypesSelected);
            trTypes.value = sl;
        }
        if (productNaturesSelected) {
            let sl = JSON.parse(productNaturesSelected);
            natures.value = sl;
        }
        if (companiesSelected) {
            let sl = JSON.parse(companiesSelected);
            companies.value = sl;
        }

    };
    const initializeSelector = () => {
        getCompanyList();
        getTransactorTypeList();
        getWarehouseItemNatureList();
       
        $('[name=itemsSearchText]').on('keyup',
             function(event) {
                   
                    var $this = $(this);
                    console.log("Inside keyUp event");
                    searchText = $this.val();
                    
                    if (timeKey) {
                        //console.log("Clear timeKey");
                        clearTimeout(timeKey);
                    }
                    timeKey = setTimeout(function() {
                            //console.log("Will execute get request in 1 sec!");
                            if (searchText.length === 0) {
                             
                                return;
                            }
                            seekItems();
                            $('#itemSelector').modal('handleUpdate');
                        },
                        1000);
                });
    };

    const showTransactorSelector = () => {};
    const showSupplierSelector = () => {
        selectorDef = supplierSelectorDef;
        indPgLib.setIndexPageDefinition(selectorDef);
        let $modal = $('#itemSelector');
        $modal.find('.modal-title').text("Supplier selector");
        $('#TransactorSelectorTypesDiv').show();
        $('#MaterialNatureTypesDiv').hide();
        let currentCompany = $('#ItemVm_CompanyId').val();
        $searchTextElement.val("");
        clearTable();
        //var multiSelect = document.getElementById("TransactorTypes").ej2_instances[0];
        let sl = [3];
        transactorTypesSelected = JSON.stringify(sl);
        //multiSelect.value = sl;
        //multiSelect.refresh();
        seekItems = seekSuppliers;
        itemSelected = transactorSelected;
        // getTransactorTypeList();
    };
    const showCustomerSelector = () => {
        selectorDef = customerSelectorDef;
        indPgLib.setIndexPageDefinition(selectorDef);
        let $modal = $('#itemSelector');
        $modal.find('.modal-title').text("Customer selector");
        $('#TransactorSelectorTypesDiv').show();
        $('#MaterialNatureTypesDiv').hide();
        let currentCompany = $('#ItemVm_CompanyId').val();
        $searchTextElement.val("");
        clearTable();
       // var multiSelect = document.getElementById("TransactorTypes").ej2_instances[0];
        let sl = [2];
        transactorTypesSelected = JSON.stringify(sl);
        //multiSelect.value = sl;
        //multiSelect.refresh();
        seekItems = seekCustomers;
        itemSelected = transactorSelected;
        // getTransactorTypeList();
    };
    const showProductSelector = (productSelectionCallback) => {
        selectorDef = materialSelectorDef;
        indPgLib.setIndexPageDefinition(selectorDef);
        let $modal = $('#itemSelector');
        $modal.find('.modal-title').text("Product selector");
        $('#TransactorSelectorTypesDiv').hide();
        $('#MaterialNatureTypesDiv').show();
        let currentCompany = $('#ItemVm_CompanyId').val();
        $searchTextElement.val("");
        clearTable();
        //var multiSelect = document.getElementById("MaterialNatureTypes").ej2_instances[0];
        seekItems = seekProducts;
        itemSelected = productSelectionCallback;
    };
    return {
        initializeSelector:initializeSelector,
        showTransactorSelector: showTransactorSelector,
        showSupplierSelector:showSupplierSelector,
        showCustomerSelector:showCustomerSelector,
        showProductSelector: showProductSelector,
        updateFilterControls: updateFilterControls
    };
})();