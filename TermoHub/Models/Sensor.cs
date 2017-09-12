using System.Collections.Generic;

namespace TermoHub.Models
{
    public class Sensor
    {
        public int DeviceId { get; set; }
        public int SensorId { get; set; }
        public string Name { get; set; }
        public int? AlertId { get; set; }
        public string Unit { get; set; }

        public Device Device { get; set; }
        public ICollection<Reading> Readings { get; set; }
        public Alert Alert { get; set; }
    }
}
