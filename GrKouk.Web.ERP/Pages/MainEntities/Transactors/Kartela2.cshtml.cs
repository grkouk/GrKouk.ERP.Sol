using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.MainEntities.Transactors
{
    [Authorize(Roles = "Admin")]
    public class KartelaModel2 : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public KartelaModel2(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public decimal sumCredit = 0;
        public decimal sumDebit = 0;

        public int PageSizeKartela { get; set; }
        public string TransactorName { get; set; }
        public int TransactorId { get; set; }

        public int ParentPageSize { get; set; }
        public int TransactorTypeFilter { get; set; }
        public int ParentPageIndex { get; set; }
        public PagedList<KartelaLine> ListItems { get; set; }
        public async Task OnGetAsync(int transactorId, int? pageIndexKartela, int? pageSizeKartela, string transactorName, int? transactorTypeFilter, int? parentPageIndex, int? parentPageSize)
        {
            TransactorTypeFilter = (int)(transactorTypeFilter ?? 0);
            ParentPageSize = (int)(parentPageSize ?? 0);
            ParentPageIndex = (int)(parentPageIndex ?? 0);
            TransactorId = transactorId;

            TransactorName = transactorName;
            PageSizeKartela = (int)((pageSizeKartela == null || pageSizeKartela == 0) ? 20 : pageSizeKartela);

            var dbTransactions = _mapper.Map<IEnumerable<TransactorTransListDto>>(await _context.TransactorTransactions
                .Include(p => p.Transactor)
                .Include(p => p.TransTransactorDocSeries)
                .OrderBy(p => p.TransDate)
                .Where(p=>p.TransactorId==TransactorId)
                .ToListAsync());



            var listWithTotal = new List<KartelaLine>();

            decimal runningTotal = 0;
            foreach (var dbTransaction in dbTransactions)
            {
                runningTotal = dbTransaction.CreditAmount - dbTransaction.DebitAmount + runningTotal;
                listWithTotal.Add(new KartelaLine
                {
                    TransDate = dbTransaction.TransDate,
                    DocSeriesCode = dbTransaction.TransTransactorDocSeriesCode,
                    RunningTotal = runningTotal,
                    TransactorName = dbTransaction.TransactorName,
                    Debit = dbTransaction.DebitAmount,
                    Credit = dbTransaction.CreditAmount
                });
            }

            var outList = listWithTotal.AsQueryable();
           

            IQueryable<KartelaLine> fullListIq = from s in outList select s;

            ListItems = PagedList<KartelaLine>.Create(
                fullListIq, pageIndexKartela ?? 1, PageSizeKartela);

            foreach (var item in ListItems)
            {
                sumCredit += item.Credit;
                sumDebit += item.Debit;
            }



        }
    }
}