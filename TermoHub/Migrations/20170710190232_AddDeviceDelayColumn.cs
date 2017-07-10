using Microsoft.EntityFrameworkCore.Migrations;

namespace TermoHub.Migrations
{
    public partial class AddDeviceDelayColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DelaySeconds",
                schema: "dbo",
                table: "Devices",
                nullable: false,
                defaultValue: 30);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelaySeconds",
                schema: "dbo",
                table: "Devices");
        }
    }
}
