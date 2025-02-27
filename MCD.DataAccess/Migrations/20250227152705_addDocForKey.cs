using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addDocForKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SharedDocuments_DocumentId",
                table: "SharedDocuments",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_SharedDocuments_Documents_DocumentId",
                table: "SharedDocuments",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SharedDocuments_Documents_DocumentId",
                table: "SharedDocuments");

            migrationBuilder.DropIndex(
                name: "IX_SharedDocuments_DocumentId",
                table: "SharedDocuments");
        }
    }
}
