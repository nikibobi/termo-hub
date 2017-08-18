using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TermoHub.Models;

namespace TermoHub.Migrations
{
    [DbContext(typeof(TermoHubContext))]
    [Migration("20170818141751_AddAlertIsNotifiedColumn")]
    partial class AddAlertIsNotifiedColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasDefaultSchema("dbo")
                .HasAnnotation("ProductVersion", "1.1.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("TermoHub.Models.Alert", b =>
                {
                    b.Property<int>("AlertId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(320)
                        .IsUnicode(true);

                    b.Property<bool>("IsNotified")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<double>("Limit");

                    b.Property<int>("Sign");

                    b.HasKey("AlertId");

                    b.ToTable("Alerts");
                });

            modelBuilder.Entity("TermoHub.Models.Device", b =>
                {
                    b.Property<int>("DeviceId");

                    b.Property<int>("DelaySeconds")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(30);

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

                    b.Property<int?>("AlertId");

                    b.Property<string>("Name")
                        .HasMaxLength(80)
                        .IsUnicode(true);

                    b.HasKey("DeviceId", "SensorId");

                    b.HasIndex("AlertId")
                        .IsUnique();

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
                    b.HasOne("TermoHub.Models.Alert", "Alert")
                        .WithOne("Sensor")
                        .HasForeignKey("TermoHub.Models.Sensor", "AlertId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("TermoHub.Models.Device", "Device")
                        .WithMany("Sensors")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
