using System.Collections.Generic;
using GrKouk.Erp.Definitions;
using Newtonsoft.Json;
using Syncfusion.EJ2.Base;

namespace GrKouk.Web.ERP.Helpers
{
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
        public int WarehouseItemId { get; set; }
        public string ClientProfileFilter { get; set; }
        public string CashRegCategoryFilter { get; set; }
        public int DisplayCurrencyId { get; set; }
        public string CodeToCompute { get; set; }
        public int DocumentId { get; set; }
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

}
