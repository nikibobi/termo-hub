using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
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

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Expiration = TimeSpan.FromDays(30);
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/error";
            });

            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new CsvOutputFormatter());
                options.FormatterMappings.SetMediaTypeMappingForFormat("csv", "text/csv");
            });

            services.AddSingleton<ILastValues, LastValues>();
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
                    name: "default",
                    template: "{controller=Main}/{action=Index}");
            });
        }
    }
}
