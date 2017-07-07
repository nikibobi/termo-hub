using System;
using Microsoft.AspNetCore.Mvc;
using TermoHub.Models;
using System.Threading.Tasks;

namespace TermoHub
{
    public class ApiController : Controller
    {
        private const int SleepSeconds = 10;

        private readonly TermoHubContext context;

        public ApiController(TermoHubContext context)
        {
            this.context = context;
        }

        // POST /new
        [Route("new")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody][Bind("DeviceId,SensorId,Value")]Reading reading)
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        Device device = await context.Devices.FindAsync(reading.DeviceId);
                        if (device == null)
                        {
                            device = new Device()
                            {
                                DeviceId = reading.DeviceId
                            };
                            await context.Devices.AddAsync(device);
                        }
                        Sensor sensor = await context.Sensors.FindAsync(reading.DeviceId, reading.SensorId);
                        if (sensor == null)
                        {
                            sensor = new Sensor()
                            {
                                DeviceId = reading.DeviceId,
                                SensorId = reading.SensorId
                            };
                            await context.Sensors.AddAsync(sensor);
                        }
                        
                        await context.Readings.AddAsync(reading);
                        await context.SaveChangesAsync();
                        transaction.Commit();
                        return Ok(SleepSeconds);
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }
            return BadRequest();
        }
    }
}
