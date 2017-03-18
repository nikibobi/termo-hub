using System;

namespace TermoHub.Models
{
    public class Reading
    {
        public int DeviceId { get; set; }
        public int SensorId { get; set; }
        public DateTime Time { get; set; }
        public double Value { get; set; }

        public Sensor Sensor { get; set; }
    }
}
