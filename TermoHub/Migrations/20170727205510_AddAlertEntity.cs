using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TermoHub.Migrations
{
    public partial class AddAlertEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlertId",
                schema: "dbo",
                table: "Sensors",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Alerts",
                schema: "dbo",
                columns: table => new
                {
                    AlertId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(maxLength: 320, nullable: false),
                    Limit = table.Column<double>(nullable: false),
                    Sign = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.AlertId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_AlertId",
                schema: "dbo",
                table: "Sensors",
                column: "AlertId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Alerts_AlertId",
                schema: "dbo",
                table: "Sensors",
                column: "AlertId",
                principalSchema: "dbo",
                principalTable: "Alerts",
                principalColumn: "AlertId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Alerts_AlertId",
                schema: "dbo",
                table: "Sensors");

            migrationBuilder.DropTable(
                name: "Alerts",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Sensors_AlertId",
                schema: "dbo",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "AlertId",
                schema: "dbo",
                table: "Sensors");
        }
    }
}
