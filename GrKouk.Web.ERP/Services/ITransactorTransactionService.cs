using System.Threading.Tasks;
using GrKouk.Erp.Dtos.TransactorTransactions;

namespace GrKouk.Web.ERP.Services;

public interface ITransactorTransactionService
{
    Task<ServiceResult> AddTransactorTransaction(TransactorTransCreateDto itemVm,string  callerSectionCode);
    Task<ServiceResult> ModifyTransactorTransaction(TransactorTransModifyDto itemVm);
    Task<ServiceResult> DeleteTransactorTransaction(int id);
}