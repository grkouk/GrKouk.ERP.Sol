var definitionsLib = (function() {
    const indexPageTypeEnum = {
        IndexPage: 1,
        ProductSelector: 20,

        TransactorSelector: 50,
        SupplierSelector: 51,
        CustomerSelector: 52
       
    };
    Object.freeze(indexPageTypeEnum);
    return {
        IndexPageTypeEnum:indexPageTypeEnum
    };
})();