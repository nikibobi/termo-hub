using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace TermoHub.Authorization
{
    using Models;

    public class DeviceOwnerHandler : AuthorizationHandler<DeviceOwnedRequirement, Device>
    {
        private readonly UserManager<User> userManager;

        public DeviceOwnerHandler(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext auth, DeviceOwnedRequirement requirement, Device device)
        {
            var user = await userManager.GetUserAsync(auth.User);
            if (device.OwnerId == user?.Id)
            {
                auth.Succeed(requirement);
            }
        }
    }
}
