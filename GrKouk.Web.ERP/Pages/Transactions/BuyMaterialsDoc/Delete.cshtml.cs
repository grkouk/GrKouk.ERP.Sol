using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.BuyDocuments;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Transactions.BuyMaterialsDoc
{
    [Authorize(Roles = "Admin, Operator")]
    public class DeleteModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        public DeleteModel(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [BindProperty]
        public BuyDocModifyDto ItemVm { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var buyMatDoc = await _context.BuyDocuments
                .Include(b => b.Company)
                .Include(b => b.FiscalPeriod)
                .Include(b => b.BuyDocSeries)
                .Include(b => b.BuyDocType)
                .Include(b => b.Section)

                .Include(b => b.Transactor)
                .Include(b => b.BuyDocLines)
                .ThenInclude(m => m.WarehouseItem)
                .FirstOrDefaultAsync(m => m.Id == id);

            ItemVm = _mapper.Map<BuyDocModifyDto>(buyMatDoc);

            if (ItemVm == null)
            {
                return NotFound();
            }
            //check for new values or old values
            var vmLines = ItemVm.BuyDocLines;
            foreach (var vmLine in vmLines)
            {
                if (vmLine.TransactionUnitId==0)
                {
                    vmLine.TransactionUnitId = vmLine.PrimaryUnitId;
                    vmLine.TransactionUnitFactor = 1;
                    vmLine.TransUnitPrice = vmLine.UnitPrice;
                    vmLine.TransactionQuantity = vmLine.Quontity1;
                }

            }
            
            LoadCombos();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            // const string sectionCode = "SYS-BUY-MATERIALS-SCN";
            if (id == null)
            {
                return NotFound();
            }
           
            var buyDocument = await _context.BuyDocuments.FindAsync(id);

            if (buyDocument != null)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.BuyDocLines.RemoveRange(_context.BuyDocLines.Where(p => p.BuyDocumentId == id));
                    _context.TransactorTransactions.RemoveRange(_context.TransactorTransactions.Where(p => p.CreatorSectionId == buyDocument.SectionId && p.CreatorId == id));
                    _context.CashFlowAccountTransactions.RemoveRange(_context.CashFlowAccountTransactions.Where(p => p.CreatorSectionId == buyDocument.SectionId && p.CreatorId == id));
                    _context.WarehouseTransactions.RemoveRange(_context.WarehouseTransactions.Where(p => p.SectionId == buyDocument.SectionId && p.CreatorId == id));
                    _context.BuyDocTransPaymentMappings.RemoveRange(_context.BuyDocTransPaymentMappings.Where(p=>p.BuyDocumentId==id));
                    var syncDoc = await _context.SyncBuyDocuments.SingleOrDefaultAsync(p => p.ErpId == buyDocument.Id);
                    if (syncDoc is not null)
                    {
                        var syncLog = await _context.SynchronizationLogs.SingleOrDefaultAsync(p => p.EntityId == syncDoc.Id);
                        if (syncLog is not null)
                        {
                            _context.SynchronizationLogs.Remove(syncLog);
                        }
                        _context.SyncBuyDocuments.Remove(syncDoc);
                    }
                    _context.BuyDocuments.Remove(buyDocument);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    string msg = $"Error  {ex.Message} inner exception->{ex.InnerException?.Message}";
                    ModelState.AddModelError(string.Empty, msg);
                    //LoadCombos();
                    return Page();
                }
               
            }

            return RedirectToPage("./Index");
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
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.OrderBy(p => p.Name).AsNoTracking(), "Id", "Name");
        }
    }
}
