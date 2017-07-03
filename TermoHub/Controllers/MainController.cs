using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TermoHub.Models;
using TermoHub.ViewModels;
using System.Collections.Generic;

namespace TermoHub
{
    public class MainController : Controller
    {
        private readonly TermoHubContext context;

        public MainController(TermoHubContext context)
        {
            this.context = context;
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
                Value = (from r in context.Readings
                         where r.DeviceId == d.DeviceId
                         orderby r.Time ascending
                         select r.Value).LastOrDefault()
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
                    foreach (var sensor in device.Sensors)
                    {
                        context.Entry(sensor)
                            .Collection(s => s.Readings)
                            .Load();
                    }

                    ViewData["Title"] = device.NameOrId();
                    var cards = device.Sensors.Select(s => new Card()
                    {
                        Title = s.NameOrId(),
                        Id = s.SensorId,
                        Url = $"/{s.DeviceId}/{s.SensorId}",
                        Value = (from r in s.Readings
                                 orderby r.Time ascending
                                 select r.Value).LastOrDefault()
                    });
                    return View(model: cards);
            }
        }

        // GET: /devId/senId
        [Route("/{devId}/{senId}")]
        public IActionResult GetSensor([FromRoute] int devId, [FromRoute] int senId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            ViewData["from"] = from.ToUtcString();
            ViewData["to"] = to.ToUtcString();
            return HandleSensor(devId, senId);
        }

        // GET: /devId/senId/live
        [Route("/{devId}/{senId}/live")]
        public IActionResult GetSensorLive([FromRoute] int devId, [FromRoute] int senId)
        {
            return HandleSensor(devId, senId);
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
        [Route("/{devId}/{senId}/data")]
        public IEnumerable<TimeValuePair<double>> GetData([FromRoute] int devId, [FromRoute] int senId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            from = from.GetValueOrDefault().ToUniversalTime();
            to = to.GetValueOrDefault(DateTime.Now).ToUniversalTime();
            return from r in context.Readings
                   where r.DeviceId == devId
                   where r.SensorId == senId
                   let time = r.Time.ToUniversalTime()
                   where @from < time && time < @to
                   select new TimeValuePair<double>(time, r.Value);
        }

        [Route("/error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
