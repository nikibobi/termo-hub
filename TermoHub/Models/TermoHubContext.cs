using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TermoHub.Extensions;

namespace TermoHub.Models
{
    public class TermoHubContext : DbContext
    {
        public TermoHubContext(DbContextOptions<TermoHubContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<Reading> Readings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<Device>(device =>
            {
                device.Property(d => d.DeviceId)
                    .ValueGeneratedNever();

                device.Property(d => d.Name).IsName();

                device.HasKey(d => d.DeviceId);

                device
                    .HasMany(d => d.Sensors)
                    .WithOne(s => s.Device)
                    .HasForeignKey(s => s.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Sensor>(sensor =>
            {
                sensor.Property(s => s.SensorId)
                    .ValueGeneratedNever();

                sensor.Property(s => s.Name).IsName();

                sensor.HasKey(s => new { s.DeviceId, s.SensorId });

                sensor
                    .HasMany(s => s.Readings)
                    .WithOne(r => r.Sensor)
                    .HasForeignKey(r => new { r.DeviceId, r.SensorId })
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Reading>(reading =>
            {
                reading.Property(r => r.Time)
                    .HasDefaultValueSql("GETDATE()");

                reading.HasKey(r => new { r.DeviceId, r.SensorId, r.Time });
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
