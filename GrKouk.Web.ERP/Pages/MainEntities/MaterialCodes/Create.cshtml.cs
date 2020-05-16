using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Dtos.WarehouseItems;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.MainEntities.MaterialCodes
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
       #region Fields
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public int CopyFromId { get; set; }
        #endregion
        public CreateModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }
        public async Task<IActionResult> OnGetAsync(int? copyFromId)
        {
           await LoadCombos();
            CopyFromId = 0;
            if (copyFromId != null)
            {
                CopyFromId = (int)copyFromId;
                var itemToCopy = await _context.WrItemCodes
                    .FirstOrDefaultAsync(m => m.Id == CopyFromId);

                if (itemToCopy == null)
                {
                    return NotFound();
                }

                //WarehouseItemVm = _mapper.Map<WarehouseItemCreateDto>(itemToCopy);
                ItemVm = WrItemCodeCreateDto.Map( itemToCopy);

            }


            return Page();
        }

        private async Task LoadCombos()
        {
            List<SelectListItem> codeTypes = new List<SelectListItem>
            {
                new SelectListItem() {Value = WarehouseItemCodeTypeEnum.CodeTypeEnumCode.ToString(), Text = "Code"},
                new SelectListItem() {Value = WarehouseItemCodeTypeEnum.CodeTypeEnumBarcode.ToString(), Text = "Barcode"},
                new SelectListItem() {Value = WarehouseItemCodeTypeEnum.CodeTypeEnumSupplierCode.ToString(), Text = "Supplier Code"}
            };
            ViewData["CodeType"] = new SelectList(codeTypes, "Value", "Text");

            List<SelectListItem> codeUsedUnits = new List<SelectListItem>
            {
                new SelectListItem() {Value = WarehouseItemCodeUsedUnitEnum.CodeUsedUnitEnumMain.ToString(), Text = "Main Unit"},
                new SelectListItem() {Value = WarehouseItemCodeUsedUnitEnum.CodeUsedUnitEnumBuy.ToString(), Text = "Buy Unit"},
                new SelectListItem() {Value = WarehouseItemCodeUsedUnitEnum.CodeUsedUnitEnumSecondary.ToString(), Text = "Secondary Unit"}
            };
            ViewData["CodeUsedUnit"] = new SelectList(codeUsedUnits, "Value", "Text");

            ViewData["WarehouseItemId"] = new SelectList(_context.WarehouseItems.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            //ViewData["CompanyId"] = FiltersHelper.GetCompaniesFilterList(_context);
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["TransactorId"] = await FiltersHelper.GetTransactorsForTypeFilterListAsync(_context, "SYS.SUPPLIER");
        }
        [BindProperty]
        public WrItemCodeCreateDto ItemVm { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.WrItemCodes.Add(ItemVm.MapToEntity());
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}