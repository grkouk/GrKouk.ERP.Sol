using System.Collections.Generic;
using System.Threading.Tasks;
using GrKouk.Erp.Dtos.SellDocuments;
using Microsoft.AspNetCore.Mvc;

namespace GrKouk.Web.ERP.ViewComponents
{
    public class SellDocTableSectionViewComponent : ViewComponent
    {   
        public async Task<IViewComponentResult> InvokeAsync(List<SellDocLineModifyDto> sellDocLineList)
        {
            return View(sellDocLineList);
        }
    }
}