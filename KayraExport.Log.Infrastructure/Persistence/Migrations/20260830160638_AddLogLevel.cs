using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KayraExport.Log.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLogLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "EventLogs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Information");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_Level",
                table: "EventLogs",
                column: "Level");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventLogs_Level",
                table: "EventLogs");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "EventLogs");
        }
    }
}
