using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TermoHub.Models;

namespace TermoHub.Migrations
{
    [DbContext(typeof(TermoHubContext))]
    partial class TermoHubContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasDefaultSchema("dbo")
                .HasAnnotation("ProductVersion", "1.1.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TermoHub.Models.Device", b =>
                {
                    b.Property<int>("DeviceId");

                    b.Property<string>("Name")
                        .HasMaxLength(80)
                        .IsUnicode(true);

                    b.HasKey("DeviceId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("TermoHub.Models.Reading", b =>
                {
                    b.Property<int>("DeviceId");

                    b.Property<int>("SensorId");

                    b.Property<DateTime>("Time")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<double>("Value");

                    b.HasKey("DeviceId", "SensorId", "Time");

                    b.ToTable("Readings");
                });

            modelBuilder.Entity("TermoHub.Models.Sensor", b =>
                {
                    b.Property<int>("DeviceId");

                    b.Property<int>("SensorId");

                    b.Property<string>("Name")
                        .HasMaxLength(80)
                        .IsUnicode(true);

                    b.HasKey("DeviceId", "SensorId");

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("TermoHub.Models.Reading", b =>
                {
                    b.HasOne("TermoHub.Models.Sensor", "Sensor")
                        .WithMany("Readings")
                        .HasForeignKey("DeviceId", "SensorId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TermoHub.Models.Sensor", b =>
                {
                    b.HasOne("TermoHub.Models.Device", "Device")
                        .WithMany("Sensors")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
