using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class returntheforkeyofentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "Entities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Entities_DocumentId",
                table: "Entities",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entities_Documents_DocumentId",
                table: "Entities",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entities_Documents_DocumentId",
                table: "Entities");

            migrationBuilder.DropIndex(
                name: "IX_Entities_DocumentId",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Entities");
        }
    }
}
