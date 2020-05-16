using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GrKouk.Erp.Domain.Shared;
using GrKouk.Erp.Dtos.TransactorTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.MainEntities.Transactors
{
    [Authorize(Roles = "Admin")]
    public class Isozigio3Model : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public Isozigio3Model(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public decimal sumCredit = 0;
        public decimal sumDebit = 0;
        public decimal sumDifferense = 0;
        public int PageSizeKartela { get; set; }
        public string TransactorName { get; set; }
        public int TransactorId { get; set; }

        public int ParentPageSize { get; set; }
       public int ParentPageIndex { get; set; }
        public int TransactorTypeFilter { get; set; }
        public int CurrentTransactorTypeFilter { get; set; }
        public int CompanyFilter { get; set; }
        public string IsozigioName { get; set; }

        public PagedList<KartelaLine> ListItems { get; set; }
        public async Task OnGetAsync(int transactorId, int? pageIndexKartela, int? pageSizeKartela, string transactorName, int? transactorTypeFilter, int? companyFilter, int? parentPageIndex, int? parentPageSize)
        {
            LoadFilters();
            TransactorTypeFilter = (int)(transactorTypeFilter ?? 0);
            CompanyFilter = (int) (companyFilter ?? 0);

            if (transactorTypeFilter == null)
            {
                transactorTypeFilter = 0;
            }
            CurrentTransactorTypeFilter = (int)(transactorTypeFilter ?? 0);
            ParentPageSize = (int)(parentPageSize ?? 0);
            ParentPageIndex = (int)(parentPageIndex ?? 0);
            TransactorId = transactorId;

            TransactorName = transactorName;
            PageSizeKartela = (int)((pageSizeKartela == null || pageSizeKartela == 0) ? 20 : pageSizeKartela);


           
            IQueryable<TransactorTransaction> transactionsList = _context.TransactorTransactions;
            if (transactorTypeFilter > 0)
            {
                transactionsList = transactionsList.Where(p => p.Transactor.TransactorTypeId == transactorTypeFilter);
            }
            if (companyFilter > 0)
            {
                transactionsList = transactionsList.Where(p => p.CompanyId == companyFilter);
            }
            var dbTrans =transactionsList.ProjectTo<TransactorTransListDto>(_mapper.ConfigurationProvider);

            var dbTransactions = dbTrans.GroupBy(g => new
                    {
                        g.CompanyCode, g.TransactorName
                    }
                )
                .Select(s => new
                {
                    TransactorName = s.Key.TransactorName,
                    CompanyCode = s.Key.CompanyCode,
                    DebitAmount = s.Sum(x => x.DebitAmount),
                    CreditAmount = s.Sum(x => x.CreditAmount)
                }).ToList();

            var isozigioType = "FREE";
            var transactorType = await _context.TransactorTypes.Where(c => c.Id  == transactorTypeFilter).FirstOrDefaultAsync();
            IsozigioName = "";
            if (transactorType!=null)
            {
                switch (transactorType.Code)
                {
                    case "SYS.DTRANSACTOR":
                        IsozigioName = "Συναλλασόμενων Ημερολογίου";
                        isozigioType = "SUPPLIER";
                        break;
                    case "SYS.CUSTOMER":
                        IsozigioName = "Πελατών";
                        isozigioType = "CUSTOMER";
                        break;
                    case "SYS.SUPPLIER":
                        IsozigioName = "Προμηθευτών";
                        isozigioType = "SUPPLIER";
                        break;

                }
               
            }
            var listWithTotal = new List<KartelaLine>();

            decimal runningTotal = 0;
            foreach (var dbTransaction in dbTransactions)
            {
                switch (isozigioType)
                {
                    case "SUPPLIER":
                        runningTotal = dbTransaction.CreditAmount - dbTransaction.DebitAmount ;
                        break;
                    case "CUSTOMER":
                        runningTotal = dbTransaction.DebitAmount - dbTransaction.CreditAmount ;
                        break;
                    default:
                        runningTotal = dbTransaction.CreditAmount - dbTransaction.DebitAmount ;
                        break;
                }
               
                listWithTotal.Add(new KartelaLine
                {
                   
                    RunningTotal = runningTotal,
                    TransactorName = dbTransaction.TransactorName,
                    CompanyCode=dbTransaction.CompanyCode,
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
            switch (isozigioType)
            {
                case "SUPPLIER":
                    sumDifferense = sumCredit - sumDebit;
                    break;
                case "CUSTOMER":
                    sumDifferense = sumDebit - sumCredit;
                    break;
                default:
                    sumDifferense = sumCredit - sumDebit;
                    break;
            }


        }
        private void LoadFilters()
        {
            var dbTransactorTypes = _context.TransactorTypes.OrderBy(p => p.Code).AsNoTracking();
            List<SelectListItem> transactorTypes = new List<SelectListItem>();
            transactorTypes.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Types}" });
            foreach (var dbTransactorType in dbTransactorTypes)
            {
                transactorTypes.Add(new SelectListItem() { Value = dbTransactorType.Id.ToString(), Text = dbTransactorType.Code });
            }
            ViewData["TransactorTypeId"] = new SelectList(transactorTypes, "Value", "Text");

            var dbCompanies = _context.Companies.OrderBy(p => p.Code).AsNoTracking();
            List<SelectListItem> companiesList = new List<SelectListItem>();
            companiesList.Add(new SelectListItem() { Value = 0.ToString(), Text = "{All Companies}" });
            foreach (var company in dbCompanies)
            {
                companiesList.Add(new SelectListItem() { Value = company.Id.ToString(), Text = company.Code });
            }
            ViewData["CompanyFilter"] = new SelectList(companiesList, "Value", "Text");
        }
    }
}