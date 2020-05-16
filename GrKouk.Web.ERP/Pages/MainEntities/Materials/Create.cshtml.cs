using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.WarehouseItems;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.MainEntities.Materials
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public int CopyFromId { get; set; }
        public CreateModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> OnGetAsync(int? copyFromId)
        {
            LoadCombos();
            CopyFromId = 0;
            if (copyFromId != null)
            {
                CopyFromId = (int)copyFromId;
                var materialToModify = await _context.WarehouseItems
                    .Include(m => m.BuyMeasureUnit)
                    .Include(m => m.Company)
                    .Include(m => m.FpaDef)
                    .Include(m => m.MainMeasureUnit)
                    .Include(m => m.MaterialCaterory)
                    .Include(m => m.SecondaryMeasureUnit).FirstOrDefaultAsync(m => m.Id == CopyFromId);

                if (materialToModify == null)
                {
                    return NotFound();
                }

                WarehouseItemVm = _mapper.Map<WarehouseItemCreateDto>(materialToModify);

            }


            return Page();
        }

        private void LoadCombos()
        {

            List<SelectListItem> materialTypes = new List<SelectListItem>
            {
                new SelectListItem() {Value = MaterialTypeEnum.MaterialTypeNormal.ToString(), Text = "Κανονικό"},
                new SelectListItem() {Value = MaterialTypeEnum.MaterialTypeSet.ToString(), Text = "Set"},
                new SelectListItem() {Value = MaterialTypeEnum.MaterialTypeComposed.ToString(), Text = "Συντιθέμενο"}
            };
            var materialNatures = Enum.GetValues(typeof(WarehouseItemNatureEnum))
                .Cast<WarehouseItemNatureEnum>()
                .Select(c => new SelectListItem()
                {
                    Value = c.ToString(),
                    Text = c.GetDescription()
                }).ToList();
            ViewData["MaterialNatures"] = new SelectList(materialNatures, "Value", "Text");

            ViewData["BuyMeasureUnitId"] = new SelectList(_context.MeasureUnits.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["FpaDefId"] = new SelectList(_context.FpaKategories.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["MainMeasureUnitId"] = new SelectList(_context.MeasureUnits.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["MaterialCategoryId"] = new SelectList(_context.MaterialCategories.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["SecondaryMeasureUnitId"] = new SelectList(_context.MeasureUnits.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["MaterialType"] = new SelectList(materialTypes, "Value", "Text");
           // ViewData["CashRegCategoryId"] = new SelectList(_context.CashRegCategories.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
        }

        [BindProperty]
        public WarehouseItemCreateDto WarehouseItemVm { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var materialToAttach = _mapper.Map<WarehouseItem>(WarehouseItemVm);
            _context.WarehouseItems.Add(materialToAttach);
            try
            {
                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("WarehouseItem Created");
                return RedirectToPage("./Index");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _toastNotification.AddErrorToastMessage(e.Message);
                return Page();
            }

          
        }
    }
}