using System.ComponentModel;

namespace GrKouk.Erp.Definitions
{

    public enum RecurringDocTypeEnum
    {
        [Description("Buy Type")]
        BuyType =1,
        [Description("Sell Type")]
        SellType =2
    }
    public enum DiaryTransactionsKindEnum
    {
        Expence = 1,
        Income = 2
    }

    /// <summary>
    /// Material Nature
    /// Υλικό, Υπηρεσία, Πάγιο, Δαπάνη
    /// </summary>
    public enum WarehouseItemNatureEnum
    {
        [Description("Χωρίς Προσδιορισμό")]
        WarehouseItemNatureUndefined = 0,
        [Description("Υλικό")]
        WarehouseItemNatureMaterial = 1,
        [Description("Υπηρεσία")]
        WarehouseItemNatureService = 2,
        [Description("Δαπάνη")]
        WarehouseItemNatureExpense = 3,
        [Description("Εσοδο")]
        WarehouseItemNatureIncome = 4,
        [Description("Πάγιο")]
        WarehouseItemNatureFixedAsset = 5,
        [Description("Πρώτη Υλη")]
        WarehouseItemNatureRawMaterial = 6
    }
    public enum MaterialTypeEnum
    {
        [Description("Κανονικο")]
        MaterialTypeNormal = 1,
        [Description("Σετ")]
        MaterialTypeSet = 2,
        [Description("Συντιθέμενο")]
        MaterialTypeComposed = 3

    }

    /// <summary>
    /// Deprecated
    /// Τύπος κίνησης
    /// 1=Χρεωση
    /// 2=Πίστωση
    /// 
    /// </summary>
    public enum FinancialTransactionTypeEnum
    {
        FinancialTransactionTypeIgnore = 0,
        FinancialTransactionTypeDebit = 1,
        FinancialTransactionTypeCredit = 2
    }
    /// <summary>
    /// Deprecated
    /// Τύπος κίνησης Αποθήκης
    /// 1=Εισαγωγή
    /// 2=Εξαγωγή
    /// 
    /// </summary>
    public enum WarehouseTransactionTypeEnum
    {
        WarehouseTransactionTypeIgnore = 0,
        WarehouseTransactionTypeImport = 1,
        WarehouseTransactionTypeExport = 2
    }


    /// <summary>
    /// Warehouse Transaction New Type Enum
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum InventoryActionEnum
    {
        [Description("Καμία Μεταβολή")]
        InventoryActionEnumNoChange = 0,
        [Description("Εισαγωγή")]
        InventoryActionEnumImport = 1,
        [Description("Εξαγωγή")]
        InventoryActionEnumExport = 2,
        [Description("Αρνητική Εισαγωγή")]
        InventoryActionEnumNegativeImport = 3,
        [Description("Αρνητική Εξαγωγή")]
        InventoryActionEnumNegativeExport = 4
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum InventoryValueActionEnum
    {
        [Description("Καμία Μεταβολή")]
        InventoryValueActionEnumNoChange = 0,
        [Description("Αυξηση")]
        InventoryValueActionEnumIncrease = 1,
        [Description("Μείωση")]
        InventoryValueActionEnumDecrease = 2,
        [Description("Αρνητική Αύξηση")]
        InventoryValueActionEnumNegativeIncrease = 3,
        [Description("Αρνητική Μείωση")]
        InventoryValueActionEnumNegativeDecrease = 4
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum FinActionsEnum
    {
        [Description("Καμία Μεταβολή")]
        FinActionsEnumNoChange = 0,
        [Description("Χρέωση")]
        FinActionsEnumDebit = 1,
        [Description("Πίστωση")]
        FinActionsEnumCredit = 2,
        [Description("Αρνητική Χρέωση")]
        FinActionsEnumNegativeDebit = 3,
        [Description("Αρνητική Πίστωση")]
        FinActionsEnumNegativeCredit = 4
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum PriceTypeEnum
    {
        [Description("Καθαρή Τιμή")]
        PriceTypeEnumNetto = 1,
        [Description("Μικτή Τιμή")]
        PriceTypeEnumBrutto = 2,
        [Description("Τελ.Τιμή Αγοράς")]
        PriceTypeEnumLastBuy = 3,
        [Description("Τιμή Κόστους")]
        PriceTypeEnumCost = 4
    }

    public enum WarehouseItemCodeTypeEnum
    {
        [Description("Κωδικό")]
        CodeTypeEnumCode = 1,
        [Description("Barcode")]
        CodeTypeEnumBarcode = 2,
        [Description("Κωδ.Προμ.")]
        CodeTypeEnumSupplierCode = 3
    }

    public enum WarehouseItemCodeUsedUnitEnum
    {
        [Description("Κύρια")]
        CodeUsedUnitEnumMain = 1,
        [Description("Δευτερεύουσα")]
        CodeUsedUnitEnumSecondary = 2,
        [Description("Αγορών")]
        CodeUsedUnitEnumBuy = 3
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum DiaryTypeEnum
    {
        [Description("Πωλήσεις")]
        DiaryTypeEnumSales = 1,
        [Description("Αγορές")]
        DiaryTypeEnumBuys = 2,
        [Description("Δαπάνες")]
        DiaryTypeEnumExpenses = 3,
        [Description("Εσοδα")]
        DiaryTypeEnumIncome = 4,
        [Description("Κινήσεις Αποθήκης")]
        DiaryTypeEnumWarehouse = 5,
        [Description("Κινήσεις Συναλλασσόμενων")]
        DiaryTypeEnumTransactors = 6
    }
    public enum MainInfoSourceTypeEnum
    {
        [Description("Πωλήσεις")]
        SourceTypeSales = 1,
        [Description("Αγορές")]
        SourceTypeBuys = 2,
        [Description("Κινήσεις Αποθήκης")]
        SourceTypeWarehouseTransactions = 5,
        [Description("Κινήσεις Συναλλασσόμενων")]
        SourceTypeTransactorTransactions = 6
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum InfoEntityActionEnum
    {
        [Description("No change")]
        InfoEntityActionEnumNoChange = 1,
        [Description("Increase")]
        InfoEntityActionEnumIncrease = 2,
        [Description("Decrease")]
        InfoEntityActionEnumDecrease = 3,
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum PictureEntityEnum
    {
        [Description("Product")]
        PictureEntityEnumProduct = 1

    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum SeriesAutoPayoffEnum
    {
        [Description("No Payoff")]
        SeriesAutoPayoffEnumNoPayoff = 0,
        [Description("Auto Payoff")]
        SeriesAutoPayoffEnumAuto = 1,
        [Description("Question")]
        SeriesAutoPayoffEnumNoQuestion = 2
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum ProdCodeNature
    {
        [Description("Product")]
        ProdCodeNatureProduct = 1,
        [Description("Service")]
        ProdCodeNatureService = 2,
        [Description("Expense")]
        ProdCodeNatureExpense = 3,
        [Description("Income")]
        ProdCodeNatureIncome = 4,
        [Description("Fixed Asset")]
        ProdCodeNatureFixedAsset = 5,
        [Description("Raw Material")]
        ProdCodeNatureRaw = 6
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum ProdCodeRawShape
    {
        [Description("Bead")]
        ProdCodeRawShapeBead = 10,
        [Description("Νήμα")]
        ProdCodeRawShapeNima = 20,
        [Description("Figure")]
        ProdCodeRawShapeFigure = 30
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1712:Do not prefix enum values with type name", Justification = "<Pending>")]
    public enum ProdCodeSeason
    {
        [Description("Season 2018")]
        ProdCodeSeason2018 = 18,
        [Description("Season 2019")]
        ProdCodeSeason2019 = 19,
        [Description("Season 2020")]
        ProdCodeSeason2020 = 20
    }
    
}
