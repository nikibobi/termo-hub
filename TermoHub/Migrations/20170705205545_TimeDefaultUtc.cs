using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TermoHub.Migrations
{
    public partial class TimeDefaultUtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>("Time", "Readings", defaultValueSql: "GETUTCDATE()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>("Time", "Readings", defaultValueSql: "GETDATE()");
        }
    }
}
