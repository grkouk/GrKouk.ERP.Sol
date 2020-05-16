using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Dtos.WarehouseTransactions;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.MainEntities.Materials
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

        public decimal sumImportsVolume = 0;
        public decimal sumExportsVolume = 0;
        public decimal sumImportsValue = 0;
        public decimal sumExportsValue = 0;

        public int PageSizeKartela { get; set; }
        public string TransactorName { get; set; }
        public int TransactorId { get; set; }

        public int ParentPageSize { get; set; }
        public int TransactorTypeFilter { get; set; }
        public int ParentPageIndex { get; set; }
        public PagedList<WarehouseKartelaLine> ListItems { get; set; }
        public async Task OnGetAsync(int transactorId, int? pageIndexKartela, int? pageSizeKartela, string transactorName
            , int? transactorTypeFilter, int? parentPageIndex, int? parentPageSize)
        {
            TransactorTypeFilter = (int)(transactorTypeFilter ?? 0);
            ParentPageSize = (int)(parentPageSize ?? 0);
            ParentPageIndex = (int)(parentPageIndex ?? 0);
            TransactorId = transactorId;

            TransactorName = transactorName;
            PageSizeKartela = (int)((pageSizeKartela == null || pageSizeKartela == 0) ? 20 : pageSizeKartela);

            var dbTransactions = _mapper.Map<IEnumerable<WarehouseTransListDto>>(await _context.WarehouseTransactions
                .Include(p => p.WarehouseItem)
                .Include(p => p.TransWarehouseDocSeries)
                .OrderBy(p => p.TransDate)
                .Where(p=>p.WarehouseItemId==TransactorId)
                .ToListAsync());



            var listWithTotal = new List<WarehouseKartelaLine>();

            decimal runningTotalVolume = 0;
            decimal runningTotalValue = 0;
            foreach (var dbTransaction in dbTransactions)
            {
                runningTotalVolume = dbTransaction.ImportUnits - dbTransaction.ExportUnits + runningTotalVolume;
                runningTotalValue = dbTransaction.ImportAmount - dbTransaction.ExportAmount + runningTotalValue;
                listWithTotal.Add(new WarehouseKartelaLine
                {
                    TransDate = dbTransaction.TransDate,
                    DocSeriesCode = dbTransaction.TransWarehouseDocSeriesCode,
                    RunningTotalVolume = runningTotalVolume,
                    RunningTotalValue = runningTotalValue,
                    MaterialName = dbTransaction.WarehouseItemName,
                    ImportVolume=dbTransaction.ImportUnits,
                    ExportVolume = dbTransaction.ExportUnits,
                    ImportValue = dbTransaction.ImportAmount,
                    ExportValue = dbTransaction.ExportAmount
                });
            }

            var outList = listWithTotal.AsQueryable();
           

            IQueryable<WarehouseKartelaLine> fullListIq = from s in outList select s;

            ListItems = PagedList<WarehouseKartelaLine>.Create(
                fullListIq, pageIndexKartela ?? 1, PageSizeKartela);

            foreach (var item in ListItems)
            {
                sumImportsVolume += item.ImportVolume;
                sumExportsVolume += item.ExportVolume;
                sumImportsValue += item.ImportValue;
                sumExportsValue += item.ExportValue;
            }



        }
    }
}