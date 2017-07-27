using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TermoHub.Extensions;

namespace TermoHub.Models
{
    public class TermoHubContext : DbContext
    {
        private const int DefaultDelaySeconds = 30;

        public TermoHubContext(DbContextOptions<TermoHubContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<Reading> Readings { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<Device>(device =>
            {
                device.Property(d => d.DeviceId)
                    .ValueGeneratedNever();

                device.Property(d => d.Name).IsName();

                device.Property(d => d.DelaySeconds)
                    .HasDefaultValue(DefaultDelaySeconds);

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

            modelBuilder.Entity<Alert>(alert =>
            {
                alert.Property(a => a.AlertId)
                    .ValueGeneratedOnAdd();

                alert.Property(a => a.Email)
                    .IsUnicode()
                    .HasMaxLength(320)
                    .IsRequired();

                alert.HasKey(a => a.AlertId);

                alert
                    .HasOne(a => a.Sensor)
                    .WithOne(s => s.Alert)
                    .HasForeignKey<Sensor>(s => s.AlertId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
