using System.Threading.Tasks;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Erp.Dtos.SellDocuments;
using Microsoft.AspNetCore.Mvc;

namespace GrKouk.Web.ERP.Services;

public interface IDocumentTransactionService
{
    Task<IActionResult> AddBuyDocument(BuyDocCreateAjaxDto docTrans);
    Task<IActionResult> ModifyBuyDocument(BuyDocModifyAjaxDto docTrans);
    Task<ServiceResult> DeleteBuyDocument(int docId);
    Task<IActionResult> AddSalesDoc(SellDocCreateAjaxDto docTrans);
    Task<IActionResult> ModifySalesDoc(SellDocModifyAjaxDto docTrans);
    Task<ServiceResult> DeleteSaleDocument(int docId);
}