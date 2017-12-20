using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace TermoHub.Authorization
{
    using Models;

    public class DeviceAdminHandler : AuthorizationHandler<DeviceOwnedRequirement, Device>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext auth, DeviceOwnedRequirement requirement, Device device)
        {
            if (auth.User.IsInRole(Role.Admin))
            {
                auth.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
