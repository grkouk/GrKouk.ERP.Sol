using System;
using System.Diagnostics;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrKouk.Web.ERP.Pages.Settings
{
    public class AppInfoModel : PageModel
    {
        private readonly ApiDbContext _context;

        public AppInfoModel(ApiDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public ApplicationInformation AppInfo { get; set; }

        public void OnGet()
        {
            Debug.WriteLine("Inside AppInfo Page OnGet");
            try
            {
                AppInfo = new ApplicationInformation
                {
                    DatabaseName = "#Unknown#",
                    DatabaseServer = "#Unknown#"
                };

                var databaseName = _context.Database.GetDbConnection().Database;
                var databaseServer = _context.Database.GetDbConnection().DataSource;
                AppInfo.DatabaseName = databaseName;
                AppInfo.DatabaseServer = databaseServer;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Inside AppInfo Page OnGet catch clause");
                Debug.WriteLine(e.Message);

            }

        }
    }
}