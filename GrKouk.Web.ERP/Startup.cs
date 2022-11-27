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
        
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt/QHNqVVhkW1pFdEBBXHxAd1p/VWJYdVt5flBPcDwsT3RfQF9iSHxUdk1iXX5adnFdRQ==;Mgo+DSMBPh8sVXJ0S0V+XE9AcVRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS3xSd0dkWXdfdHVRQGdcUg==;ORg4AjUWIQA/Gnt2VVhjQlFaclhJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRd0RhXn9WcXBUQmZUUEc=;NzY5MTcyQDMyMzAyZTMzMmUzME56ZzQxbzUrVERSREtGTllCeHBUZzVUMW9UWUxWWVNYU0NITCs2bmk0RjA9;NzY5MTczQDMyMzAyZTMzMmUzMGxNWjQ3c2JGODNDdC9sN1pUVXdTZDJ5RDFPeHhod1MzTW4yTXpNMUZWUHM9;NRAiBiAaIQQuGjN/V0Z+X09EaFtFVmJLYVB3WmpQdldgdVRMZVVbQX9PIiBoS35RdERiW3heeHVRR2RbVUB1;NzY5MTc1QDMyMzAyZTMzMmUzMGxVMWxzNGpwVmJiaDMzdFBITnQ5RkpEbHFxdVVxanZnTjdKd0dNdE81czg9;NzY5MTc2QDMyMzAyZTMzMmUzMFFuQklmZDcwd3g0dGoreE1scmJsRVFGWjIxTzVGSUlzWnFxamI3VWVqMW89;Mgo+DSMBMAY9C3t2VVhjQlFaclhJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRd0RhXn9WcXBUQmhcUkc=;NzY5MTc4QDMyMzAyZTMzMmUzMGs0M1h4T3ZZZUhHYlRxNVlwS0p1bjlsWE1YVHdERWorVEF6bHNCZkhPWHM9;NzY5MTc5QDMyMzAyZTMzMmUzMEZYSHczdnNpMjVUNmZPMjBpSlJ4SHBuNnVQOU9kUjhUNEd2TVg3c2RRSDQ9;NzY5MTgwQDMyMzAyZTMzMmUzMGxVMWxzNGpwVmJiaDMzdFBITnQ5RkpEbHFxdVVxanZnTjdKd0dNdE81czg9");
        }
    }
}
