using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TermoHub.Models;
using TermoHub.ViewModels;
using TermoHub.Extensions;
using TermoHub.Services;

namespace TermoHub.Controllers
{
    public class SensorController : Controller
    {
        private static readonly TimeSpan HistoryInterval = TimeSpan.FromHours(3);

        private readonly TermoHubContext context;
        private readonly ILastValues lastValues;

        public SensorController(TermoHubContext context, ILastValues lastValues)
        {
            this.context = context;
            this.lastValues = lastValues;
        }

        // GET: /sensors
        [HttpGet("/sensors")]
        public IActionResult List()
        {
            var cards = context.Sensors.Select(s => new Card()
            {
                Title = s.NameOrId(),
                Id = s.SensorId,
                Url = $"/{s.DeviceId}/{s.SensorId}",
                Value = lastValues.GetSensorLastValue(s.DeviceId, s.SensorId)
            });
            ViewData["Title"] = "Sensors";
            return View(model: cards);
        }

        // GET: /devId/senId
        [HttpGet("/{devId}/{senId}")]
        public IActionResult Show([FromRoute] int devId, [FromRoute] int senId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            Sensor sensor = context.Sensors.Find(devId, senId);
            if (sensor == null)
                return NotFound();

            (ViewData["from"], ViewData["to"]) = DefaultDates(from, to);

            return View(model: sensor);
        }

        // GET: /devId/senId/live
        [HttpGet("/{devId}/{senId}/live")]
        public IActionResult Live([FromRoute] int devId, [FromRoute] int senId)
        {
            Sensor sensor = context.Sensors.Find(devId, senId);
            if (sensor == null)
                return NotFound();

            return View(model: sensor);
        }

        // GET: /devId/senId/settings
        [HttpGet("/{devId}/{senId}/settings")]
        public IActionResult Edit([FromRoute] int devId, [FromRoute] int senId)
        {
            Sensor sensor = context.Sensors.Find(devId, senId);
            if (sensor == null)
                return NotFound();

            context.Entry(sensor).Reference(s => s.Alert).Load();
            return View(model: sensor);
        }

        // POST: /devId/senId/settings
        [HttpPost("/{devId}/{senId}/settings")]
        public IActionResult Update([FromRoute] int devId, [FromRoute] int senId, [FromForm] string name, [FromForm] bool hasAlert, [FromForm] Alert alert)
        {
            Sensor sensor = context.Sensors.Find(devId, senId);
            if (sensor == null)
                return NotFound();

            context.Entry(sensor).Reference(s => s.Alert).Load();
            sensor.Name = name;
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
        [HttpGet("/{devId}/{senId}/alert")]
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

        private static (DateTime from, DateTime to) DefaultDates(DateTime? fromNullable, DateTime? toNullable)
        {
            DateTime to = toNullable.GetValueOrDefault(DateTime.Now);
            DateTime from = fromNullable.GetValueOrDefault(to.Subtract(HistoryInterval));
            return (from, to);
        }
    }
}