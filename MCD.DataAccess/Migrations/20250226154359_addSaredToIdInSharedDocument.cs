using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSaredToIdInSharedDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SharedToId",
                table: "SharedDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharedToId",
                table: "SharedDocuments");
        }
    }
}
