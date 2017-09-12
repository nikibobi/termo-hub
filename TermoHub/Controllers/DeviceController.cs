using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TermoHub.Services;
using TermoHub.Models;
using TermoHub.ViewModels;
using TermoHub.Extensions;

namespace TermoHub.Controllers
{
    public class DeviceController : Controller
    {
        private readonly TermoHubContext context;
        private readonly ILastValues lastValues;

        public DeviceController(TermoHubContext context, ILastValues lastValues)
        {
            this.context = context;
            this.lastValues = lastValues;
        }

        // GET: /devices
        [HttpGet("/devices")]
        public IActionResult List()
        {
            var cards = context.Devices.Select(d => new Card()
            {
                Title = d.NameOrId(),
                Id = d.DeviceId,
                Url = $"/{d.DeviceId}"
            });
            ViewData["Title"] = "TermoHub";
            return View(model: cards);
        }

        // GET: /devId
        [HttpGet("/{devId}")]
        public IActionResult Show([FromRoute] int devId)
        {
            Device device = context.Devices.Find(devId);
            if (device == null)
                return NotFound();

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
        [HttpGet("/{devId}/settings")]
        public IActionResult Edit([FromRoute] int devId)
        {
            Device device = context.Devices.Find(devId);
            if (device == null)
                return NotFound();

            return View(model: device);
        }

        // POST: /devId/settings
        [HttpPost("/{devId}/settings")]
        public IActionResult Update([FromRoute] int devId, [FromForm] string name, [FromForm] int delaySeconds)
        {
            Device device = context.Devices.Find(devId);
            if (device == null)
                return NotFound();

            device.Name = name;
            device.DelaySeconds = delaySeconds;
            context.Update(device);
            context.SaveChanges();
            return Redirect($"/{devId}");
        }
    }
}