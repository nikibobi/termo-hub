using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TermoHub.Models;

namespace TermoHub.Services
{
    public class LastValues : ILastValues
    {
        private readonly IDictionary<(int, int), double> cache;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public LastValues(IServiceScopeFactory serviceScopeFactory)
        {
            cache = new ConcurrentDictionary<(int, int), double>();
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public double GetSensorLastValue(int deviceId, int sensorId)
        {
            if (!cache.TryGetValue((deviceId, sensorId), out double value))
            {
                using (var scope = serviceScopeFactory.CreateScope())
                using (var context = scope.ServiceProvider.GetRequiredService<TermoHubContext>())
                {
                    value = context.Readings
                        .Where(r => r.DeviceId == deviceId)
                        .Where(r => r.SensorId == sensorId)
                        .OrderByDescending(r => r.Time)
                        .Select(r => r.Value)
                        .FirstOrDefault();
                    SetSensorLastValue(deviceId, sensorId, value);
                }
            }
            return value;
        }

        public void SetSensorLastValue(int deviceId, int sensorId, double value)
        {
            cache[(deviceId, sensorId)] = value;
        }
    }
}
