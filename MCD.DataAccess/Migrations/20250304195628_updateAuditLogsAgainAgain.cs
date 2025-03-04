using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateAuditLogsAgainAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "userEmailAddress",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "userEmailAddress",
                table: "AuditLogs");
        }
    }
}
