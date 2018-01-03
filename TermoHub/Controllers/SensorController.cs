using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TermoHub.Models;
using TermoHub.ViewModels;
using TermoHub.Extensions;
using TermoHub.Services;

namespace TermoHub.Controllers
{
    [Authorize]
    public class SensorController : Controller
    {
        private static readonly TimeSpan HistoryInterval = TimeSpan.FromHours(3);

        private readonly TermoHubContext context;
        private readonly ILastValues lastValues;
        private readonly IAuthorizationService authorization;

        public SensorController(TermoHubContext context, ILastValues lastValues, IAuthorizationService authorization)
        {
            this.context = context;
            this.lastValues = lastValues;
            this.authorization = authorization;
        }

        // GET: /sensors
        [HttpGet]
        public IActionResult List()
        {
            var cards = context.Sensors
                .Where(IsAuthorized)
                .Select(s => new Card()
                {
                    Title = s.NameOrId(),
                    Id = s.SensorId,
                    Url = $"/{s.DeviceId}/{s.SensorId}",
                    Value = lastValues.GetSensorLastValue(s.DeviceId, s.SensorId),
                    Unit = s.Unit
                });
            ViewData["Title"] = "Sensors";
            return View(model: cards);
        }

        // GET: /devId/senId
        [HttpGet]
        public IActionResult Show([FromRoute] int devId, [FromRoute] int senId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            Sensor sensor = context.Sensors.Find(devId, senId);
            if (sensor == null)
                return NotFound();

            if (!IsAuthorized(sensor))
                return Forbid();

            (ViewData["from"], ViewData["to"]) = DefaultDates(from, to);

            return View(model: sensor);
        }

        // GET: /devId/senId/live
        [HttpGet]
        public IActionResult Live([FromRoute] int devId, [FromRoute] int senId)
        {
            Sensor sensor = context.Sensors.Find(devId, senId);
            if (sensor == null)
                return NotFound();

            if (!IsAuthorized(sensor))
                return Forbid();

            return View(model: sensor);
        }

        // GET: /devId/senId/settings
        [HttpGet]
        public IActionResult Settings([FromRoute] int devId, [FromRoute] int senId)
        {
            Sensor sensor = context.Sensors.Find(devId, senId);
            if (sensor == null)
                return NotFound();

            if (!IsAuthorized(sensor))
                return Forbid();

            context.Entry(sensor).Reference(s => s.Alert).Load();
            return View(model: sensor);
        }

        // POST: /devId/senId/settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings([FromRoute] int devId, [FromRoute] int senId, [FromForm] string name, [FromForm] string unit, [FromForm] bool hasAlert, [FromForm] Alert alert)
        {
            Sensor sensor = context.Sensors.Find(devId, senId);
            if (sensor == null)
                return NotFound();

            if (!IsAuthorized(sensor))
                return Forbid();

            context.Entry(sensor).Reference(s => s.Alert).Load();
            sensor.Name = name;
            sensor.Unit = unit;
            if (hasAlert && sensor.Alert != null)
            {
                sensor.Alert.Sign = alert.Sign;
                sensor.Alert.Limit = alert.Limit;
                sensor.Alert.Email = alert.Email;
            }
            else if (hasAlert && sensor.Alert == null)
            {
                sensor.Alert = alert;
            }
            else if (!hasAlert && sensor.Alert != null)
            {
                context.Remove(sensor.Alert);
            }
            context.Update(sensor);
            context.SaveChanges();
            return Redirect($"/{devId}/{senId}");
        }

        // GET: /devId/senId/data?from=&to=
        [FormatFilter]
        [HttpGet("/{devId}/{senId}/data.{format}")]
        public IEnumerable<TimeValuePair<double>> Data([FromRoute] int devId, [FromRoute] int senId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var dates = DefaultDates(from, to);
            DateTime fromUtc = dates.from.ToUniversalTime();
            DateTime toUtc = dates.to.ToUniversalTime();
            return from r in context.Readings
                   where r.DeviceId == devId
                   where r.SensorId == senId
                   where fromUtc < r.Time && r.Time < toUtc
                   select new TimeValuePair<double>(r.Time.ToLocalTime(), r.Value);
        }

        // GET: /devId/senId/alert
        [HttpGet]
        public IActionResult Alert([FromRoute] int devId, [FromRoute] int senId)
        {
            Sensor sensor = context.Sensors.Find(devId, senId);
            if (sensor == null)
                return NotFound();

            context.Entry(sensor).Reference(s => s.Alert).Load();
            Alert alert = sensor.Alert;
            if (alert == null)
                return Json(data: null);

            return Json(data: new { Value = alert.Limit, Sign = alert.Sign });
        }

        private bool IsAuthorized(Sensor sensor)
        {
            context.Entry(sensor).Reference(s => s.Device).Load();
            var result = authorization.AuthorizeAsync(User, sensor.Device, "DeviceOwned").GetAwaiter().GetResult();
            return result.Succeeded;
        }

        private static (DateTime from, DateTime to) DefaultDates(DateTime? fromNullable, DateTime? toNullable)
        {
            DateTime to = toNullable.GetValueOrDefault(DateTime.Now);
            DateTime from = fromNullable.GetValueOrDefault(to.Subtract(HistoryInterval));
            return (from, to);
        }
    }
}