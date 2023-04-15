using System.Collections.Generic;
using GrKouk.Erp.Definitions;
using Newtonsoft.Json;
using Syncfusion.EJ2.Base;

namespace GrKouk.Web.ERP.Helpers
{
    /// <summary>
    /// Index page request object. Includes all filter and sort values for selecting index page table rows 
    /// </summary>
    public class IndexDataTableRequest
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SortData { get; set; }
        public string DateRange { get; set; }
        public string CompanyFilter { get; set; }
        public string WarehouseItemNatureFilter { get; set; }
        public string TransactorTypeFilter { get; set; }
        public string SearchFilter { get; set; }
        public int DiaryId { get; set; }
        public int TransactorId { get; set; }
        public int CashFlowAccountId { get; set; }
        public int WarehouseItemId { get; set; }
        public string ClientProfileFilter { get; set; }
        public string CashRegCategoryFilter { get; set; }
        public int DisplayCurrencyId { get; set; }
        public string CodeToCompute { get; set; }
        public int DocumentId { get; set; }
        public bool ShowCarryOnAmountsInTabs { get; set; }
        public bool ShowSummaryFilter { get; set; }
        public bool ShowDisplayLinesWithZeroes { get; set; }
    }

    public class IndexDataTableResponse<T>
    {
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public decimal SumOfAmount { get; set; }
        public decimal SumOfNetAmount { get; set; }
        public decimal SumOfVatAmount { get; set; }
        public decimal SumOfPayedAmount { get; set; }
        public decimal SumOfDebit { get; set; }
        public decimal SumOfCredit { get; set; }
        public decimal SumOfDifference { get; set; }
        public decimal SumImportVolume { get; set; }
        public decimal SumExportVolume { get; set; }
        public decimal SumImportValue { get; set; }
        public decimal SumExportValue { get; set; }
        public decimal GrandSumOfAmount { get; set; }
        public decimal GrandSumOfNetAmount { get; set; }
        public decimal GrandSumOfVatAmount { get; set; }
        public decimal GrandSumOfPayedAmount { get; set; }
        public decimal GrandSumOfDebit { get; set; }
        public decimal GrandSumOfCredit { get; set; }
        public decimal GrandSumOfDifference { get; set; }
        public decimal GrandSumImportVolume { get; set; }
        public decimal GrandSumExportVolume { get; set; }
        public decimal GrandSumImportValue { get; set; }
        public decimal GrandSumExportValue { get; set; }
        public List<T> Data { get; set; }
        public List<SearchListItem> Diaries { get; set; }

    }

    public class CodeToComputeDefinition
    {
        public MainInfoSourceTypeEnum SrcType { get; set; }
        public string[] MatNatures { get; set; }
        public int[] TransTypes { get; set; }
        public string[] CompSelected { get; set; }
        public int[] DocTypesSelected { get; set; }

    }

    public class ExtendedDataManagerRequest : DataManagerRequest
    {

        [JsonProperty(PropertyName = "transactorId", Required = Required.Default)]
        public int? TransactorId { get; set; }
        [JsonProperty(PropertyName = "displayCurrencyId", Required = Required.Default)]
        public int DisplayCurrencyId { get; set; }
        [JsonProperty(PropertyName = "companyFilter", Required = Required.Default)]
        public string CompanyFilter { get; set; }
        [JsonProperty(PropertyName = "dateRange", Required = Required.Default)]
        public string DateRange { get; set; }
    }

    public class AutoCompleteDataManagerRequest : DataManagerRequest
    {

        [JsonProperty(PropertyName = "searchMode", Required = Required.Default)]
        public int SearchMode { get; set; }
        [JsonProperty(PropertyName = "searchTerm", Required = Required.Default)]
        public string SearchTerm { get; set; }
        [JsonProperty(PropertyName = "companyId", Required = Required.Default)]
        public int CompanyId { get; set; }
        [JsonProperty(PropertyName = "seriesId", Required = Required.Default)]
        public int SeriesId { get; set; }
    }

    public class Ej2AutoCompleteItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
        public string ImgUrl { get; set; }
    }
}
