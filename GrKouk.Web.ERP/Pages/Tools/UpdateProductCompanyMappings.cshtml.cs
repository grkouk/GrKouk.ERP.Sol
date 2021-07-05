using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.ERP.Pages.Tools
{
    public class UpdateProductCompanyMappingsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public UpdateProductCompanyMappingsModel(ApiDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {
        }
        public void OnPost()
        {
            var wiList = _context.WarehouseItems;
            foreach (var item in wiList)
            {
                var wrId = item.Id;
                var cmpId = item.CompanyId;
                var mpList = _context.CompanyWarehouseItemMappings.Where(p=>p.CompanyId==cmpId && p.WarehouseItemId == wrId).ToList();
                if (mpList.Count == 0)
                {
                    var cmpMap = new CompanyWarehouseItemMapping()
                    {
                        CompanyId = cmpId,
                        WarehouseItemId = wrId
                    };
                    _context.CompanyWarehouseItemMappings.Add(cmpMap);
                   
                }
            }
            _context.SaveChanges();
        }
    }
}
