using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TermoHub.Models
{
    public static class ModelExtensions
    {
        public static string NameOrId(this Device d) => d.Name ?? $"Device #{d.DeviceId}";

        public static string NameOrId(this Sensor s) => s.Name ?? $"Sensor #{s.SensorId}";

        public static void IsName(this PropertyBuilder<string> builder)
        {
            builder
                .IsUnicode(true)
                .HasMaxLength(80);
        }
    }
}
