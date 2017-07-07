using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using TermoHub.Models;

namespace TermoHub.Services
{
    public class LastValues : ILastValues
    {
        private readonly IDictionary<int, IDictionary<int, double>> cache;

        public LastValues(TermoHubContext context)
        {
            cache = new ConcurrentDictionary<int, IDictionary<int, double>>();
            LoadFromDatabase(context);
        }

        private void LoadFromDatabase(TermoHubContext context)
        {
            foreach (var sensor in context.Sensors)
            {
                context.Entry(sensor).Collection(s => s.Readings).Load();
                double last = sensor.Readings
                    .OrderByDescending(r => r.Time)
                    .Select(r => r.Value)
                    .FirstOrDefault();
                SetSensorLastValue(sensor.DeviceId, sensor.SensorId, last);
            }
        }

        public double GetDeviceLastValuesAverage(int deviceId)
        {
            if (cache.ContainsKey(deviceId) && cache[deviceId].Values.Count > 0)
            {
                return cache[deviceId].Values.Average();
            }
            return double.NaN;
        }

        public double GetSensorLastValue(int deviceId, int sensorId)
        {
            if (cache.ContainsKey(deviceId) && cache[deviceId].ContainsKey(sensorId))
            {
                return cache[deviceId][sensorId];
            }
            return double.NaN;
        }

        public void SetSensorLastValue(int deviceId, int sensorId, double value)
        {
            if (!cache.ContainsKey(deviceId))
            {
                cache[deviceId] = new ConcurrentDictionary<int, double>();
            }
            cache[deviceId][sensorId] = value;
        }
    }
}
