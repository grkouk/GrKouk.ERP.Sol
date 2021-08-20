using System;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NToastNotify;

namespace GrKouk.Web.ERP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
           
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                
            });
            services.AddDbContext<ApiDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApiDbContext>();
            services.AddMvc()
                .AddNToastNotifyToastr(new ToastrOptions()
                {
                    ProgressBar = true,
                    PositionClass = ToastPositions.BottomRight,
                    TimeOut = 5000,
                    ExtendedTimeOut = 1000
                });
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                //if the above is not workong then try this
                //options.ExpireTimeSpan = DateTime.Now.Subtract(DateTime.UtcNow).Add(TimeSpan.FromMinutes(5);
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });
            //services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddAutoMapper(typeof(Startup));
            services.AddRazorPages();
            services.AddControllers().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseNToastNotify();
            app.UseSession();
            //app.UseMvc();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapDefaultControllerRoute();
            });
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzg4NTI5QDMxMzgyZTM0MmUzMEtBYjNBQ055dFJPVjhKcHJDbW5pWHJadC9OSWp5d3BDc2MxZ3JxcUpnWTA9");
        
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDIxNDU4QDMxMzkyZTMxMmUzMERkaDZVL0JMeE9CaEFueG5adWFaTWFHcWxLalZYZXAzY1BNbXpYTmltUjg9;NDIxNDU5QDMxMzkyZTMxMmUzMGFWWTcyc2NKcDIwaVV1a3NGWExReTVyaFlKbkxSaFdrR3grUTFCanl4UjQ9;NDIxNDYwQDMxMzkyZTMxMmUzMGdMRzVFZ2lWajlWdDQ4NkJkWHlIb2ZzbjRvakpWOURUMG1HVVhJMjFNL3c9;NDIxNDYxQDMxMzkyZTMxMmUzMGJHYjVVVmM2Tml0UFpBQXZiMk9wQUhSR1VvMEw5dnJpa04wUDFPSjROODA9");
        }
    }
}
