using System;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
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
           var jwtSettings = Configuration.GetSection("JwtSettings");
           var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            // services.Configure<CookiePolicyOptions>(options =>
            // {
            //     // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //     options.CheckConsentNeeded = context => false;
            //     options.MinimumSameSitePolicy = SameSiteMode.None;
            //     
            //     
            // });
            services.AddDbContext<ApiDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<IdentityUser, IdentityRole>(options=>
            {
                options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role; // Ensure roles are stored in claims
            })
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApiDbContext>();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 
                
               
            }) .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Set to 60 minutes or your desired time
                options.SlidingExpiration = true;

                options.LoginPath = "/Identity/Account/Login";
            })

                .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"]
                };
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiPolicy", policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Admin"); // Ensures a role match
                });
                options.AddPolicy("ApiPolicy2", policy =>
                {
                    policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser(); // The user must be authenticated
                    policy.RequireRole("Admin");      // The user must have the Admin role
                });


                // Default (Web Cookie Authentication): No need to add schemes, relies on cookie
            });

           
            services.AddMvc()
                .AddNToastNotifyToastr(new ToastrOptions()
                {
                    ProgressBar = true,
                    PositionClass = ToastPositions.BottomRight,
                    TimeOut = 5000,
                    ExtendedTimeOut = 1000
                });
            // services.ConfigureApplicationCookie(options =>
            // {
            //     // Cookie settings
            //     options.Cookie.HttpOnly = true;
            //     options.Cookie.SameSite = SameSiteMode.Lax;
            //     options.ExpireTimeSpan = TimeSpan.FromHours(1);
            //     options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            //     //if the above is not workong then try this
            //     //options.ExpireTimeSpan = DateTime.Now.Subtract(DateTime.UtcNow).Add(TimeSpan.FromMinutes(5);
            //     options.LoginPath = "/Identity/Account/Login";
            //     options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            //     options.SlidingExpiration = true;
            // });
            //services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
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
            //---------------
            // app.Use(async (context, next) =>
            // {
            //     // Check if the request contains a Bearer token in the headers
            //     var authHeader = context.Request.Headers["Authorization"];
            //     if (authHeader.ToString().StartsWith("Bearer "))
            //     {
            //         // Switch scheme to JwtBearer for this request
            //       //  context.User = null; // Reset claims to avoid conflicts
            //       //  await context.RequestServices.GetRequiredService<IAuthenticationService>()
            //       //      .AuthenticateAsync(context, JwtBearerDefaults.AuthenticationScheme);
            //     }
            //
            //     await next();
            // });

            //--------------
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapDefaultControllerRoute();
            });
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("");
        
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2XVhhQlJHfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTH5Sd0RiXn9ccHFXTmNZ");
        }
    }
}
