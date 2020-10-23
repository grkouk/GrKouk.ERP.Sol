using System;
using System.Linq;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.MainEntities.Materials
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
        public WarehouseItem WarehouseItem { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WarehouseItem = await _context.WarehouseItems
                .Include(m => m.BuyMeasureUnit)
                .Include(m => m.Company)
                .Include(m => m.FpaDef)
                .Include(m => m.MainMeasureUnit)
                .Include(m => m.MaterialCaterory)
                .Include(m => m.SecondaryMeasureUnit).FirstOrDefaultAsync(m => m.Id == id);

            if (WarehouseItem == null)
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

            WarehouseItem = await _context.WarehouseItems.FindAsync(id);

            if (WarehouseItem != null)
            {
                _context.CompanyWarehouseItemMappings.RemoveRange(_context.CompanyWarehouseItemMappings.Where(p => p.WarehouseItemId == WarehouseItem.Id));
                _context.WarehouseItems.Remove(WarehouseItem);
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
                                    _toastNotification.AddErrorToastMessage("Το είδος έχει κινήσεις και δεν μπορεί να διαγραφεί");

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
