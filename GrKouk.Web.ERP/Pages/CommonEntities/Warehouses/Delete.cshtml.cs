using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Warehouses;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.CommonEntities.Warehouses
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IToastNotification _toastNotification;
        public DeleteModel(ApiDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        [BindProperty]
        public Warehouse ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ItemVm = await _context.Warehouses
                
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ItemVm == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ItemVm = await _context.Warehouses.FindAsync(id);

            if (ItemVm != null)
            {
                _context.WarehouseCompanyMappings.RemoveRange(_context.WarehouseCompanyMappings.Where(p => p.WarehouseId == ItemVm.Id));
                _context.Warehouses.Remove(ItemVm);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    if (ex.GetBaseException() is SqlException)
                    {
                        if (ex.InnerException != null)
                        {
                            int errorCode = ((SqlException)ex.InnerException).Number;
                            switch (errorCode)
                            {
                                case 2627:  // Unique constraint error
                                    break;
                                case 547:   // Constraint check violation
                                    _toastNotification.AddErrorToastMessage("Warehouse has transactions. Cannot delete");

                                    break;
                                case 2601:  // Duplicated key row error
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        //ToDo handle normal exception
                        throw;
                    }
                    
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
