using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TermoHub.Extensions
{
    public static class ApplicationBuilderStaticFilesExtensions
    {
        public static IApplicationBuilder UseStaticFilesWeb(
            this IApplicationBuilder app)
        {
            var provider = new FileExtensionContentTypeProvider();
            var keep = new[] { ".css", ".js" };
            foreach (var key in provider.Mappings.Keys.Except(keep).ToList())
            {
                provider.Mappings.Remove(key);
            }
            provider.Mappings[".map"] = "application/json";
            provider.Mappings[".ts"] = "application/x-typescript";

            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = provider
            });

            return app;
        }

        public static IApplicationBuilder UseStaticFilesArduino(
            this IApplicationBuilder app,
            string requestPath,
            string sourcePath,
            bool enableDirectoryBrowser)
        {
            var fileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", sourcePath));

            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".zip", "application/zip, application/octet-stream" },
                { ".ino", "text/plain" }
            };

            app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = requestPath,
                FileProvider = fileProvider,
                ContentTypeProvider = new FileExtensionContentTypeProvider(mapping),
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers["Content-Disposition"] = "attachment";
                }
            });

            if (enableDirectoryBrowser)
            {
                app.UseDirectoryBrowser(new DirectoryBrowserOptions()
                {
                    RequestPath = requestPath,
                    FileProvider = fileProvider
                });
            }

            return app;
        }
    }
}
