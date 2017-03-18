using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TermoHub.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Devices",
                schema: "dbo",
                columns: table => new
                {
                    DeviceId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                schema: "dbo",
                columns: table => new
                {
                    DeviceId = table.Column<int>(nullable: false),
                    SensorId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => new { x.DeviceId, x.SensorId });
                    table.ForeignKey(
                        name: "FK_Sensors_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "dbo",
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Readings",
                schema: "dbo",
                columns: table => new
                {
                    DeviceId = table.Column<int>(nullable: false),
                    SensorId = table.Column<int>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    Value = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readings", x => new { x.DeviceId, x.SensorId, x.Time });
                    table.ForeignKey(
                        name: "FK_Readings_Sensors_DeviceId_SensorId",
                        columns: x => new { x.DeviceId, x.SensorId },
                        principalSchema: "dbo",
                        principalTable: "Sensors",
                        principalColumns: new[] { "DeviceId", "SensorId" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Readings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Sensors",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Devices",
                schema: "dbo");
        }
    }
}
