using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
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

            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContextPool<TermoHubContext>(options => options.UseSqlServer(connection));

            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new CsvOutputFormatter());
                options.FormatterMappings.SetMediaTypeMappingForFormat("csv", "text/csv");
            });

            services.AddSingleton<ILastValues, LastValues>();
            services.AddTransient<IEmailSender, MailKitEmailService>();
            services.AddTransient<IAlertReporter, AlertReporter>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseStaticFilesWeb();
            app.UseStaticFilesArduino("/files", "firmware", true);

            app.UseMvc();
        }
    }
}
