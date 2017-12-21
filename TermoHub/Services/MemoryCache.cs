using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace TermoHub.Services
{
    using Models;

    public class MemoryCache : ILastValues
    {
        private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(5);

        private readonly IMemoryCache cache;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public MemoryCache(IServiceScopeFactory serviceScopeFactory, IMemoryCache cache)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.cache = cache;
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
            cache.Set((deviceId, sensorId), value, Expiration);
        }
    }
}
