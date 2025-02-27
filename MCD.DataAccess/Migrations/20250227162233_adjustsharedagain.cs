using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class adjustsharedagain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SharedDocuments_AspNetUsers_ApplicationUserId",
                table: "SharedDocuments");

            migrationBuilder.DropIndex(
                name: "IX_SharedDocuments_ApplicationUserId",
                table: "SharedDocuments");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "SharedDocuments");

            migrationBuilder.AddColumn<string>(
                name: "SharedToEmail",
                table: "SharedDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharedToEmail",
                table: "SharedDocuments");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "SharedDocuments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SharedDocuments_ApplicationUserId",
                table: "SharedDocuments",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SharedDocuments_AspNetUsers_ApplicationUserId",
                table: "SharedDocuments",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
