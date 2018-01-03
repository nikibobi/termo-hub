using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TermoHub.Controllers
{
    using Services;
    using Models;
    using ViewModels;
    using Extensions;
    using Authorization;

    [Authorize]
    public class DeviceController : Controller
    {
        private readonly TermoHubContext context;
        private readonly ILastValues lastValues;
        private readonly IAuthorizationService authorization;

        public DeviceController(TermoHubContext context, ILastValues lastValues, IAuthorizationService authorization)
        {
            this.context = context;
            this.lastValues = lastValues;
            this.authorization = authorization;
        }

        // GET: /devices
        [HttpGet]
        public IActionResult List()
        {
            var cards = context.Devices
                .Where(IsAuthorized)
                .Select(d => new Card()
                {
                    Title = d.NameOrId(),
                    Id = d.DeviceId,
                    Url = $"/{d.DeviceId}"
                });
            ViewData["Title"] = "TermoHub";
            return View(model: cards);
        }

        // GET: /devId
        [HttpGet]
        public IActionResult Show([FromRoute] int devId)
        {
            Device device = context.Devices.Find(devId);
            if (device == null)
                return NotFound();

            if (!IsAuthorized(device))
                return Forbid();

            context.Entry(device).Collection(d => d.Sensors).Load();
            ViewData["Title"] = device.NameOrId();
            var cards = device.Sensors.Select(s => new Card()
            {
                Title = s.NameOrId(),
                Id = s.SensorId,
                Url = $"/{s.DeviceId}/{s.SensorId}",
                Value = lastValues.GetSensorLastValue(s.DeviceId, s.SensorId),
                Unit = s.Unit
            });
            return View(model: cards);
        }

        // GET: /devId/settings
        [HttpGet]
        public IActionResult Settings([FromRoute] int devId)
        {
            Device device = context.Devices.Find(devId);
            if (device == null)
                return NotFound();

            if (!IsAuthorized(device))
                return Forbid();

            if (User.IsInRole(Role.Admin))
            {
                ViewBag.Owners = context.Users
                    .Select(u => new SelectListItem()
                    {
                        Value = u.Id,
                        Text = u.UserName
                    }).ToList();
            }

            return View(model: device);
        }

        // POST: /devId/settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings([FromRoute] int devId, [FromForm] string name, [FromForm] int delaySeconds, [FromForm] string ownerId)
        {
            Device device = context.Devices.Find(devId);
            if (device == null)
                return NotFound();

            if (!IsAuthorized(device))
                return Forbid();

            if (device.OwnerId != ownerId && !User.IsInRole(Role.Admin))
                return Forbid();

            device.Name = name;
            device.DelaySeconds = delaySeconds;
            device.OwnerId = ownerId;
            context.Update(device);
            context.SaveChanges();
            return Redirect($"/{devId}");
        }

        private bool IsAuthorized(Device device)
        {
            var result = authorization.AuthorizeAsync(User, device, "DeviceOwned").GetAwaiter().GetResult();
            return result.Succeeded;
        }
    }
}