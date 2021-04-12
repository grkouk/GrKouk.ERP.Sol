using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Dtos.BuyDocuments;
using Microsoft.AspNetCore.Mvc;

namespace GrKouk.Web.ERP.ViewComponents
{
    public class BuyDocTableSectionViewComponent : ViewComponent
    {   
        public async Task<IViewComponentResult> InvokeAsync(List<BuyDocLineModifyDto> buyDocLineList)
        {
            return View(buyDocLineList);
        }
    }
}