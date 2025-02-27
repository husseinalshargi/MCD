using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MCD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class removecascadeThings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SharedDocuments_AspNetUsers_ApplicationUserId",
                table: "SharedDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_SharedDocuments_AspNetUsers_ApplicationUserId",
                table: "SharedDocuments",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction); //do nothing when deleting the user
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SharedDocuments_AspNetUsers_ApplicationUserId",
                table: "SharedDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_SharedDocuments_AspNetUsers_ApplicationUserId",
                table: "SharedDocuments",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
