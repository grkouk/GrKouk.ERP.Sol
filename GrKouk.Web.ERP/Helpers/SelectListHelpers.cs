using System.Collections.Generic;
using System.Linq;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Helpers
{
    public static class SelectListHelpers
    {
        public static List<SelectListItem> GetSectionsList(ApiDbContext context)
        {
            var dbList = context.Sections.OrderBy(p => p.Name).AsNoTracking();
            List<SelectListItem> valuesList = new List<SelectListItem>();
            valuesList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{No Selection}" });
            foreach (var item in dbList)
            {
                valuesList.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.Name });
            }
            return valuesList;
        }
    }
}