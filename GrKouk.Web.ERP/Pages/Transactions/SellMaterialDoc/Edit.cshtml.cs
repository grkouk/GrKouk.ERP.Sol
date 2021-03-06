﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Dtos.SellDocuments;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.SellMaterialDoc
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public string SeekType { get; set; }
        public bool InitialLoad = true;

        public EditModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        [BindProperty]
        public SellDocModifyDto ItemVm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var sellMatDoc = await _context.SellDocuments
                .Include(b => b.Company)
                .Include(b => b.FiscalPeriod)
                .Include(b => b.SellDocSeries)
                .Include(b => b.SellDocType)
                .Include(b => b.Section)

                .Include(b => b.Transactor)
                .Include(b => b.SellDocLines)
                .ThenInclude(m => m.WarehouseItem)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sellMatDoc == null)
            {
                return NotFound();
            }
            ItemVm = _mapper.Map<SellDocModifyDto>(sellMatDoc);

            if (ItemVm == null)
            {
                return NotFound();
            }
            var vmLines = ItemVm.SellDocLines;
            foreach (var vmLine in vmLines)
            {
                if (vmLine.TransactionUnitId==0)
                {
                    vmLine.TransactionUnitId = vmLine.PrimaryUnitId;
                    vmLine.TransactionUnitFactor = 1;
                    vmLine.TransUnitPrice = vmLine.UnitPrice;
                    vmLine.TransactionQuantity = vmLine.Quontity1;
                }

                (vmLine.PrimaryUnitCode, vmLine.PrimaryUnitName) =
                    await HelperFunctions.GetMeasureUnitDetailsAsync(_context,vmLine.PrimaryUnitId);
                (vmLine.SecondaryUnitCode, vmLine.SecondaryUnitName) =
                    await HelperFunctions.GetMeasureUnitDetailsAsync(_context,vmLine.SecondaryUnitId);
                (vmLine.TransactionUnitCode, vmLine.TransactionUnitName) =
                    await HelperFunctions.GetMeasureUnitDetailsAsync(_context,vmLine.TransactionUnitId);
            }
          

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
            var transactorList = _context.Transactors.Where(s => s.TransactorType.Code == "SYS.CUSTOMER" || s.TransactorType.Code == "SYS.DEPARTMENT").OrderBy(s => s.Name).AsNoTracking();
            //ViewData["CompanyId"] = new SelectList(_context.Companies.OrderBy(p => p.Code).AsNoTracking(), "Id", "Code");
            ViewData["SellDocSeriesId"] = new SelectList(_context.SellDocSeriesDefs.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["TransactorId"] = new SelectList(transactorList, "Id", "Name");
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            ViewData["SalesChannelId"] = new SelectList(_context.SalesChannels.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
            var companiesList = FiltersHelper.GetSolidCompaniesFilterList(_context);
            ViewData["CompanyId"] = new SelectList(companiesList, "Value", "Text");
        }

       
    }
}
