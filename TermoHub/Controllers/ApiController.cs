using System;
using Microsoft.AspNetCore.Mvc;
using TermoHub.Models;
using System.Threading.Tasks;
using TermoHub.Services;

namespace TermoHub
{
    public class ApiController : Controller
    {
        private readonly TermoHubContext context;
        private readonly ILastValues lastValues;
        private readonly IAlertReporter alertReporter;

        public ApiController(TermoHubContext context, ILastValues lastValues, IAlertReporter alertReporter)
        {
            this.context = context;
            this.lastValues = lastValues;
            this.alertReporter = alertReporter;
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
                        lastValues.SetSensorLastValue(reading.DeviceId, reading.SensorId, reading.Value);
                        await alertReporter.Report(reading);
                        return Ok(device.DelaySeconds);
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
