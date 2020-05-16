using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Erp.Dtos.Media;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.MediaMng
{
    public class IndexModel2 : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public IndexModel2(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IList<MediaEntryDto> MediaEntry { get;set; }

        public async Task OnGetAsync()
        {
            var list = _mapper.Map<List<MediaEntryDto>>(await _context.MediaEntries.ToListAsync());
            foreach (var mediaItem in list)
            {
                
                mediaItem.Url = Url.Content("productimages/" + mediaItem.MediaFile);
            }
            MediaEntry = list;
        }
    }
}
