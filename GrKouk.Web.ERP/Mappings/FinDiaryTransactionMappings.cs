using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.FinDiary;

namespace GrKouk.Web.ERP.Mappings
{
    public static class FinDiaryTransactionMappings
    {
        /// <summary>Expense form model (Edit, and Create when copying from an existing expense).</summary>
        public static FinDiaryExpenceTransModifyDto ToExpenseModifyDto(this FinDiaryTransaction src) => new()
        {
            Id = src.Id,
            TransactionDate = src.TransactionDate,
            ReferenceCode = src.ReferenceCode,
            TransactorId = src.TransactorId,
            FinTransCategoryId = src.FinTransCategoryId,
            CompanyId = src.CompanyId,
            CostCentreId = src.CostCentreId,
            RevenueCentreId = src.RevenueCentreId,
            Description = src.Description,
            Kind = src.Kind,
            AmountFpa = src.AmountFpa,
            AmountNet = src.AmountNet,
            Timestamp = src.Timestamp
        };

        /// <summary>New entity from the expense Create form. Kind and RevenueCentreId are set by the caller.</summary>
        public static FinDiaryTransaction ToEntity(this FinDiaryExpenceTransModifyDto dto) => new()
        {
            TransactionDate = dto.TransactionDate,
            ReferenceCode = dto.ReferenceCode,
            TransactorId = dto.TransactorId,
            FinTransCategoryId = dto.FinTransCategoryId,
            CompanyId = dto.CompanyId,
            CostCentreId = dto.CostCentreId,
            Description = dto.Description,
            AmountFpa = dto.AmountFpa,
            AmountNet = dto.AmountNet
        };

        /// <summary>
        /// Copies the editable fields onto a loaded entity. Id, Timestamp, Kind and RevenueCentreId
        /// are not touched (the Edit form does not post Kind, so the old save reset it to 0).
        /// </summary>
        public static void ApplyTo(this FinDiaryExpenceTransModifyDto dto, FinDiaryTransaction entity)
        {
            entity.TransactionDate = dto.TransactionDate;
            entity.ReferenceCode = dto.ReferenceCode;
            entity.TransactorId = dto.TransactorId;
            entity.FinTransCategoryId = dto.FinTransCategoryId;
            entity.CompanyId = dto.CompanyId;
            entity.CostCentreId = dto.CostCentreId;
            entity.Description = dto.Description;
            entity.AmountFpa = dto.AmountFpa;
            entity.AmountNet = dto.AmountNet;
        }
    }
}
