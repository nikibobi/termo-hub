using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

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

        public static string ToUtcString(this DateTime? date)
        {
            return date?.ToString("yyyy-MM-ddTHH:mm:ss") ?? String.Empty;
        }
    }
}
