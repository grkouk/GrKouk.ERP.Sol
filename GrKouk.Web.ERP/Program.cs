using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GrKouk.Web.ERP.Data;
using GrKouk.Web.ERP.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NToastNotify;

namespace GrKouk.Web.ERP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuration
            var configuration = builder.Configuration;

            // Database
            builder.Services.AddDbContext<ApiDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services
                .AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
                })
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApiDbContext>();

            // Health checks
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    name: "database",
                    tags: new[] { "database" }
                );

            // Authentication
            var jwtSettings = configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? string.Empty);

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.SlidingExpiration = true;
                    options.LoginPath = "/Identity/Account/Login";
                    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = context =>
                        {
                            if (context.Request.Path.StartsWithSegments("/api") ||
                                context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return Task.CompletedTask;
                            }

                            context.Response.Redirect(context.RedirectUri);
                            return Task.CompletedTask;
                        }
                    };
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

            // Authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiPolicy", policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Admin");
                });
                options.AddPolicy("ApiPolicy2", policy =>
                {
                    policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme,
                        JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Admin");
                });
            });

            // MVC + Toasts
            builder.Services.AddMvc()
                .AddNToastNotifyToastr(new ToastrOptions
                {
                    ProgressBar = true,
                    PositionClass = ToastPositions.BottomRight,
                    TimeOut = 5000,
                    ExtendedTimeOut = 1000
                });

            // Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // AutoMapper, Controllers, Razor Pages
            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddRazorPages();
            builder.Services.AddControllers().AddNewtonsoftJson();

            // App services
            builder.Services.AddTransient<IDocumentTransactionService, DocumentTransactionServiceV2>();
            builder.Services.AddTransient<IDocumentSyncService, SyncBusinessDocService>();
            builder.Services.AddTransient<ITransactorTransactionService, TransactorTransactionServiceV2>();
            builder.Services.AddTransient<ICFATransactionService, CFATransactionService>();
            builder.Services.AddTransient<IWarehouseManagementSrv, WarehouseManagementSrv>();
            builder.Services.AddTransient<PdfExportService>();

            var app = builder.Build();

            // Database connectivity check (startup gate)
            // I have disabled this check for now, because it is not showing a good error message.

            #region Database connectivity check

            // try
            // {
            //     using var scope = app.Services.CreateScope();
            //     var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            //     var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            //
            //     // Option A: quick connectivity check
            //     if (!db.Database.CanConnect())
            //     {
            //         logger.LogCritical(
            //             "Cannot connect to the database using connection string 'DefaultConnection'. Aborting startup.");
            //         // Fail fast so containers/orchestrators can restart
            //         Environment.ExitCode = -1;
            //         return; // prevent app.Run()
            //     }
            //
            //     // Option B: ensure/migrate if that’s desired (comment out if not)
            //     // db.Database.Migrate();
            //
            //     logger.LogInformation("Database connectivity check passed.");
            // }
            // catch (Exception ex)
            // {
            //     // Any exception trying to resolve the context or connect
            //     var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            //     var logger = loggerFactory.CreateLogger<Program>();
            //     logger.LogCritical(ex, "Database connectivity check failed during startup.");
            //     Environment.ExitCode = -1;
            //     return; // prevent app.Run()
            // }
            #endregion
            // ... the rest of your middleware and endpoints
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseNToastNotify();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapHealthChecks("/health");
            });

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
                "Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZcc3VQRGNeVEdyXURWYEg=");
            // Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2XVhhQlJHfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTH5Sd0RiXn9ccHFXTmNZ");

            app.Run();
        }
    }
}