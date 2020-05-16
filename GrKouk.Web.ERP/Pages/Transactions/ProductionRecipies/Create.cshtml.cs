using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.ProductionRecipies
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public string SeekType { get; set; }
        public int RoutedCompanyId { get; set; }
        public int RoutedSectionId { get; set; }
        public bool InitialLoad = true;
        public int CopyFromId { get; set; }
        public CreateModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public IActionResult OnGet(int? companyFilter, int? section, int? copyFromId)
        {
            RoutedCompanyId = companyFilter ?? 0;
            RoutedSectionId = section ?? 0;
            LoadCombos();

            return Page();
        }

        private void LoadCombos()
        {
            List<SelectListItem> seekTypes = new List<SelectListItem>
            {
                new SelectListItem() {Value = "NAME", Text = "Name"},
                new SelectListItem() {Value ="CODE", Text = "Code"},
                new SelectListItem() {Value = "BARCODE", Text = "Barcode"}
            };
            ViewData["SeekType"] = new SelectList(seekTypes, "Value", "Text");

            var supplierList = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.SUPPLIER").OrderBy(s => s.Name).AsNoTracking();
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["BuyDocSeriesId"] = new SelectList(_context.BuyDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransactorId"] = new SelectList(supplierList, "Id", "Name");
        }

        [BindProperty]
        public BuyDocCreateAjaxDto ItemVm { get; set; }

        public IActionResult OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            return RedirectToPage("./Index");
        }

    }
}