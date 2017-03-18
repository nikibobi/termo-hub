using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TermoHub.Models;
using TermoHub.ViewModels;

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
        public IActionResult GetSensor([FromRoute] int devId, [FromRoute] int senId)
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

        // GET: /devId/senId/data?t=
        [Route("/{devId}/{senId}/data")]
        public IActionResult GetData([FromRoute] int devId, [FromRoute] int senId, [FromQuery] DateTime? t)
        {
            DateTime date = t.GetValueOrDefault(DateTime.MinValue);
            var data = from r in context.Readings
                       where r.DeviceId == devId
                       where r.SensorId == senId
                       where date < r.Time
                       select new { r.Time, r.Value };
            return Json(data.ToList());
        }

        [Route("/error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
