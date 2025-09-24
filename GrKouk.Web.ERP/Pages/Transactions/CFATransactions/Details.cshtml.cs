using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.CashFlowTransactions;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.Transactions.CFATransactions
{
    [Authorize(Roles = "Admin,Operator")]
    public class DetailsModel : PageModel
    {
        private const string _sectionCode = "SYS-CFA-TRANS";
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public bool NotUpdatable;
        public bool InitialLoad = true;
        public int CopyFromId { get; set; }
        public int CopyFromTransactorId { get; set; } = 0;
        public int CfaId { get; set; }
        public DetailsModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transactionToModify = await _context.CashFlowAccountTransactions
                .Include(t => t.Company)
                .Include(t => t.FiscalPeriod)
                .Include(t => t.Section)
                .Include(t => t.DocumentSeries)
                .Include(t => t.DocumentType)
                .Include(t => t.CashFlowAccount)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (transactionToModify == null)
            {
                return NotFound();
            }

            CfaId = transactionToModify.CashFlowAccountId;
            // var section = _context.Sections.SingleOrDefault(s => s.SystemName == _sectionCode);
            // if (section is null)
            // {
            //     _toastNotification.AddAlertToastMessage("CFA Transactions section not found in DB");
            //     return BadRequest();
            // }
          
            ItemVm = _mapper.Map<CfaTransactionModifyDto>(transactionToModify);
            LoadCombos();
            return Page();
        }

        [BindProperty] public CfaTransactionModifyDto ItemVm { get; set; }
        
        private void LoadCombos()
        {
            var companiesList = FiltersHelper.GetSolidCompaniesFilterList(_context);
            ViewData["CompanyId"] = new SelectList(companiesList, "Value", "Text");
           
            ViewData["DocSeriesId"] =
                new SelectList(_context.CashFlowDocSeriesDefs.OrderBy(s => s.Name).AsNoTracking(), "Id", "Name");
            
           
        }
    }
}