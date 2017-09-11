using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        public IConfigurationRoot Configuration { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            services.AddOptions();
            services.Configure<EmailOptions>(Configuration.GetSection("Email"));
            services.Configure<ReporterOptions>(Configuration.GetSection("Reporter"));

            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<TermoHubContext>(options => options.UseSqlServer(connection));

            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new CsvOutputFormatter());
                options.FormatterMappings.SetMediaTypeMappingForFormat("csv", "text/csv");
            });

            services.AddSingleton<ILastValues, LastValues>();
            services.AddTransient<IEmailSender, MailKitEmailService>();
            services.AddTransient<IAlertReporter, AlertReporter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

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
