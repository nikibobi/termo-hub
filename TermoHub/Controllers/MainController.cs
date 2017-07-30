using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TermoHub.Extensions;
using TermoHub.Models;
using TermoHub.Services;
using TermoHub.ViewModels;

namespace TermoHub
{
    public class MainController : Controller
    {
        private static readonly TimeSpan HistoryInterval = TimeSpan.FromHours(3);

        private readonly TermoHubContext context;
        private readonly ILastValues lastValues;

        public MainController(TermoHubContext context, ILastValues lastValues)
        {
            this.context = context;
            this.lastValues = lastValues;
        }

        // GET: /
        [Route("/")]
        public IActionResult Index()
        {
            var cards = context.Devices.Select(d => new Card()
            {
                Title = d.NameOrId(),
                Id = d.DeviceId,
                Url = $"/{d.DeviceId}",
                Value = lastValues.GetDeviceLastValuesAverage(d.DeviceId)
            });
            return View(model: cards);
        }

        // GET: /devId
        [Route("/{devId}")]
        public IActionResult GetDevice([FromRoute] int devId)
        {
            switch (context.Devices.Find(devId))
            {
                case null:
                    return NotFound();
                case Device device:
                    context.Entry(device)
                        .Collection(d => d.Sensors)
                        .Load();

                    ViewData["Title"] = device.NameOrId();
                    var cards = device.Sensors.Select(s => new Card()
                    {
                        Title = s.NameOrId(),
                        Id = s.SensorId,
                        Url = $"/{s.DeviceId}/{s.SensorId}",
                        Value = lastValues.GetSensorLastValue(s.DeviceId, s.SensorId)
                    });
                    return View(model: cards);
            }
        }

        // GET: /devId/settings
        [HttpGet("/{devId}/settings")]
        public IActionResult DeviceSettings([FromRoute] int devId)
        {
            switch (context.Devices.Find(devId))
            {
                case null:
                    return NotFound();
                case Device device:
                    return View(model: device);
            }
        }

        // POST: /devId/settings
        [HttpPost("/{devId}/settings")]
        public IActionResult DeviceSettings([FromRoute] int devId, [FromForm] string name, [FromForm] int delaySeconds)
        {
            switch (context.Devices.Find(devId))
            {
                case null:
                    return NotFound();
                case Device device:
                    device.Name = name;
                    device.DelaySeconds = delaySeconds;
                    context.Update(device);
                    context.SaveChanges();
                    return Redirect($"/{devId}");
            }
        }

        // GET: /devId/senId
        [Route("/{devId}/{senId}")]
        public IActionResult GetSensor([FromRoute] int devId, [FromRoute] int senId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            (ViewData["from"], ViewData["to"]) = DefaultDates(from, to);
            return HandleSensor(devId, senId);
        }

        // GET: /devId/senId/live
        [Route("/{devId}/{senId}/live")]
        public IActionResult GetSensorLive([FromRoute] int devId, [FromRoute] int senId)
        {
            return HandleSensor(devId, senId);
        }

        // GET: /devId/senId/settings
        [HttpGet("/{devId}/{senId}/settings")]
        public IActionResult SensorSettings([FromRoute] int devId, [FromRoute] int senId)
        {
            Sensor sensor = context.Sensors.Find(devId, senId);
            if (sensor == null)
                return NotFound();

            context.Entry(sensor).Reference(s => s.Alert).Load();
            return View(model: sensor);
        }

        // POST: /devId/senId/settings
        [HttpPost("/{devId}/{senId}/settings")]
        public IActionResult SensorSettings([FromRoute] int devId, [FromRoute] int senId, [FromForm] string name, [FromForm] bool hasAlert, [FromForm] Alert alert)
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

        private IActionResult HandleSensor(int devId, int senId)
        {
            switch (context.Sensors.Find(devId, senId))
            {
                case null:
                    return NotFound();
                case Sensor sensor:
                    context.Entry(sensor)
                        .Collection(s => s.Readings)
                        .Load();
                    return View(model: sensor);
            }
        }

        // GET: /devId/senId/data?from=&to=
        [FormatFilter]
        [Route("/{devId}/{senId}/data.{format}")]
        public IEnumerable<TimeValuePair<double>> GetData([FromRoute] int devId, [FromRoute] int senId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
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

        private static (DateTime from, DateTime to) DefaultDates(DateTime? fromNullable, DateTime? toNullable)
        {
            DateTime to = toNullable.GetValueOrDefault(DateTime.Now);
            DateTime from = fromNullable.GetValueOrDefault(to.Subtract(HistoryInterval));
            return (from, to);
        }

        [Route("/error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
