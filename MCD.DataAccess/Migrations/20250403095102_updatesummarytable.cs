using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updatesummarytable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "SummarizedDocuments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "SummarizedDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
