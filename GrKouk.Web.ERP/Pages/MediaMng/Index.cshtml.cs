using AutoMapper;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;

namespace GrKouk.Web.ERP.Pages.MediaMng
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;


        public IndexModel(ApiDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }


        public void OnGet()
        {
            LoadFilters();
            
           
        }
        private void LoadFilters()
        {
          

            var pageFilterSize = PageFilter.GetPageSizeFiltersSelectList();
            ViewData["PageFilterSize"] = new SelectList(pageFilterSize, "Value", "Text");

        }
    }
}
