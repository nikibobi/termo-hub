using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.Security.Claims;
using TermoHub.Authorization;
using TermoHub.Extensions;
using TermoHub.Formatters;
using TermoHub.Models;
using TermoHub.Options;
using TermoHub.Services;

namespace TermoHub
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            services.AddOptions();
            services.Configure<EmailOptions>(Configuration.GetSection("Email"));
            services.Configure<ReporterOptions>(Configuration.GetSection("Reporter"));
            services.Configure<JwtOptions>(Configuration.GetSection("JWT"));
            services.Configure<IdentityOptions>(Configuration.GetSection("Identity"));

            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<TermoHubContext>(options => options.UseSqlServer(connection));

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<TermoHubContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeviceOwned", policy =>
                    policy.AddRequirements(new DeviceOwnedRequirement()));  
            });

            services.AddScoped<IAuthorizationHandler, DeviceAdminHandler>();
            services.AddScoped<IAuthorizationHandler, DeviceOwnerHandler>();

            services.AddAuthentication()
                .AddCookie()
                .AddJwtBearer(options =>
                {
                    var jwt = new JwtOptions();
                    Configuration.Bind("JWT", jwt);

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = jwt.SecurityKey,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Expiration = TimeSpan.FromDays(30);
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/error";
            });

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.ConstraintMap["devId"] = typeof(int);
                options.ConstraintMap["senId"] = typeof(int);
            });

            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new CsvOutputFormatter());
                options.FormatterMappings.SetMediaTypeMappingForFormat("csv", "text/csv");
            });

            services.AddSingleton<ILastValues, MemoryCache>();
            services.AddTransient<IEmailSender, DotnetEmailService>();
            services.AddTransient<IAlertReporter, AlertReporter>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDatabaseSeed();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseStaticFilesWeb();
            app.UseAuthentication();
            app.UseStaticFilesArduino("/files", "firmware", true);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "account",
                    template: "{action}",
                    defaults: new { controller = "Account" });

                routes.MapRoute(
                    name: "list",
                    template: "{controller}s",
                    defaults: new { action = "List" });

                routes.MapRoute(
                    name: "device",
                    template: "{devId}/{action}",
                    defaults: new { controller = "Device", action = "Show" });

                routes.MapRoute(
                    name: "sensor",
                    template: "{devId}/{senId}/{action}",
                    defaults: new { controller = "Sensor", action = "Show" });

                routes.MapRoute(
                    name: "default",
                    template: "{action}",
                    defaults: new { controller = "Main", action = "Index" });
            });
        }
    }
}
