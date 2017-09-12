using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TermoHub.Migrations
{
    public partial class AddSensorUnitColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                schema: "dbo",
                table: "Sensors",
                maxLength: 10,
                nullable: false,
                defaultValue: "°C");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                schema: "dbo",
                table: "Sensors");
        }
    }
}
