using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GrKouk.Erp.Dtos.WarehouseTransactions;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.Erp.Pages.Configuration.WarehouseTransDef
{
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public IndexModel(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IList<TransWarehouseDefListDto> ListItemsDtos { get; set; }

        public async Task OnGetAsync()
        {
            ListItemsDtos = await _context.TransWarehouseDefs
                .ProjectTo<TransWarehouseDefListDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}
