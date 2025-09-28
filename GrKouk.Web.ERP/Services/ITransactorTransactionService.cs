using System.Threading.Tasks;
using GrKouk.Erp.Dtos.TransactorTransactions;

namespace GrKouk.Web.ERP.Services;

public interface ITransactorTransactionService
{
    Task<ServiceResult> AddTransactorTransaction(TransactorTransCreateDto itemVm,string  callerSectionCode=null);
    Task<ServiceResult> ModifyTransactorTransaction(TransactorTransModifyDto itemVm, string  callerSectionCode=null);
    Task<ServiceResult> DeleteTransactorTransaction(int id);
}