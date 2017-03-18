using System.Collections.Generic;

namespace TermoHub.Models
{
    public class Device
    {
        public int DeviceId { get; set; }
        public string Name { get; set; }

        public ICollection<Sensor> Sensors { get; set; }
    }
}
