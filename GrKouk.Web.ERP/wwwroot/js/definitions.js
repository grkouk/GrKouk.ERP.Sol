var definitionsLib = (function() {
    const indexPageTypeEnum = {
        IndexPage: 1,
        ProductSelector: 20,

        TransactorSelector: 50,
        SupplierSelector: 51,
        CustomerSelector: 52
       
    };
    const unitTypesEnum = {
        BaseUnitType : 1,
        SecondaryUnitType : 2,
        BuyUnitType : 3,
        SupplierBuyUnitType : 4
    };
    Object.freeze(indexPageTypeEnum);
    Object.freeze(unitTypesEnum);
    return {
        IndexPageTypeEnum:indexPageTypeEnum,
        UnitTypesEnum : unitTypesEnum
    };
})();