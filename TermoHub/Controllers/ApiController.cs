using System;
using Microsoft.AspNetCore.Mvc;
using TermoHub.Models;

namespace TermoHub
{
    public class ApiController : Controller
    {
        private readonly TermoHubContext context;

        public ApiController(TermoHubContext context)
        {
            this.context = context;
        }

        // POST /new
        [Route("new")]
        [HttpPost]
        public void Post([FromBody][Bind("DeviceId,SensorId,Value")]Reading reading)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        Device device = context.Devices.Find(reading.DeviceId);
                        if (device == null)
                        {
                            device = new Device()
                            {
                                DeviceId = reading.DeviceId
                            };
                            context.Devices.Add(device);
                        }
                        Sensor sensor = context.Sensors.Find(reading.DeviceId, reading.SensorId);
                        if (sensor == null)
                        {
                            sensor = new Sensor()
                            {
                                DeviceId = reading.DeviceId,
                                SensorId = reading.SensorId
                            };
                            context.Sensors.Add(sensor);
                        }
                        
                        context.Readings.Add(reading);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }
        }
    }
}
