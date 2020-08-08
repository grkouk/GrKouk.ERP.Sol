using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
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
                options.CheckConsentNeeded = context => true;
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
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });
            services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });
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
                app.UseDatabaseErrorPage();
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
            //Mjk1NzE1QDMxMzgyZTMyMmUzMGptdzZXVG10THo1clhENjR6S0QrOGNoeHNIakpEZXVNSzkzcDYxM3B6ZEE9
          //  Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjUzODczQDMxMzgyZTMxMmUzMG5UNHArR2l3MlJYaklGMlI5aTJXVnRuNnJuN1JLQVk5V1lUV0xaTDVxeGM9;MjUzODc0QDMxMzgyZTMxMmUzMFd6SVAveHB3cVZBQ0RsMUM3MXlhL0JKWjlUbU00R1ladTRlN2RsbGJXWmM9;MjUzODc1QDMxMzgyZTMxMmUzMGVIR01rLzlHYWsvUVlIYXJoSzF5RWQ3dnBxdEtmYjB2YTZRVHROQ0VnRlE9;MjUzODc2QDMxMzgyZTMxMmUzMGJ3aEw0RTBFWDBLUjFxcWIzQk0yRlNEaFdpWExYc2s4aWJIbmFxeE1aNW89;MjUzODc3QDMxMzgyZTMxMmUzMFBiSjkzSHVqMVVhRUNETmE0V1hTZG9PTTF5NDBQYTJicUVXQ2V0WXJRVlE9;MjUzODc4QDMxMzgyZTMxMmUzMGVPS0JHRmlTaTVYRm1lVWdsdTd0YnhuaHVhUnIvblM1OGt4R3g3N0o5Q289;MjUzODc5QDMxMzgyZTMxMmUzMGJuS3REbjJwV3haSWpyb29wT0ZzL2U2bXJmZ0NDaWs2R20xNERtNHVqQ1U9;MjUzODgwQDMxMzgyZTMxMmUzMEJLc2NUWGZjUC84Yms3TDJFdm9iY1BCeTJESDlYVkZvbDVsazFmUTlEYXc9;MjUzODgxQDMxMzgyZTMxMmUzMG15d2c3dXUwZEdEakhyUlMyZ0FYdGRPWHZKM05aNHdXeEN6amVVdU1ralU9;NT8mJyc2IWhia31ifWN9Z2FoYmF8YGJ8ampqanNiYmlmamlmanMDHmg0Njg8JjgTOzwnPjI6P30wPD4=;MjUzODgyQDMxMzgyZTMxMmUzMEJmeXlUUlFUelg0Wnpsa1VCS0FZaDllQkJoSzF4TWp4NVRDbFZIZDNTTWM9");
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjk1NzE1QDMxMzgyZTMyMmUzMGptdzZXVG10THo1clhENjR6S0QrOGNoeHNIakpEZXVNSzkzcDYxM3B6ZEE9");
        }
    }
}
