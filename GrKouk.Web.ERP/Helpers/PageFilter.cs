using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrKouk.Web.ERP.Helpers
{
    public static class PageFilter
    {
        public static List<SelectListItem> GetPageSizeFiltersSelectList()
        {
            List<SelectListItem> datePeriods = new List<SelectListItem>
            {
                new SelectListItem() {Value = "1", Text = "1"},
                new SelectListItem() {Value = "5", Text = "5"},
                new SelectListItem() {Value = "10", Text = "10"},
                new SelectListItem() {Value = "20", Text = "20"},
                new SelectListItem() {Value = "50", Text = "50"},
                new SelectListItem() {Value = "100", Text = "100"}

            };
            return datePeriods;
        }
    }
}
