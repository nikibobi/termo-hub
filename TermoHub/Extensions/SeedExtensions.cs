using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TermoHub.Extensions
{
    using Authorization;

    public static class SeedExtensions
    {
        public static IApplicationBuilder UseDatabaseSeed(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                AddRoles(scope.ServiceProvider, Role.All).GetAwaiter().GetResult();
            }

            return app;
        }

        private static async Task AddRoles(IServiceProvider serviceProvider, IEnumerable<string> roles)
        {
            using (var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>())
            {
                foreach (var role in roles)
                {
                    var exists = await roleManager.RoleExistsAsync(role);
                    if (!exists)
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }
        }
    }
}
