using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Tools
{
    public class UpdateProductCompanyMappingsModel : PageModel
    {
        private readonly ApiDbContext _context;

        public UpdateProductCompanyMappingsModel(ApiDbContext context)
        {
            _context = context;
        }
        public IList<string> ResultsList { get; set; }=new List<string>();
        public int allCount { get; set; } = 0;
        public int updatedCount { get; set; } = 0;
        public int errorCount { get; set; } = 0;
        public void OnGet()
        {
        }
        public void OnPost()
        {

            var wiList = _context.WarehouseItems
                .Include(p=>p.CompanyMappings)
                .ToList();
//            var noMappings = wiList.Where(p => p.CompanyMappings.Count > 0).ToList();    

            foreach (var item in wiList)
            {
                var wrId = item.Id;
                var cmpId = item.CompanyId == 0 ? 1 : item.CompanyId;
                string itemLine = $"item {item.Name} with Id {item.Id} and companyId {item.CompanyId}";
                string itemResult = "";
                allCount++;
                if (item.CompanyMappings.Count==0)
                {
                    var cmpMap = new CompanyWarehouseItemMapping()
                    {
                        CompanyId = cmpId,
                        WarehouseItemId = wrId
                    };
                    try
                    {
                        _context.CompanyWarehouseItemMappings.Add(cmpMap);
                        _context.SaveChanges();
                        updatedCount++;
                    }
                    catch (Exception ex)
                    {
                        itemResult = $"Error with {itemLine} error is {ex.Message}";
                        ResultsList.Add(itemResult);
                        errorCount++;
                    }
                }
                
             
                
              
            }
            
        }
    }
}
