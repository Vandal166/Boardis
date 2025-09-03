using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddListCardsdbset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListCard_BoardLists_BoardListId",
                table: "ListCard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ListCard",
                table: "ListCard");

            migrationBuilder.RenameTable(
                name: "ListCard",
                newName: "ListCards");

            migrationBuilder.RenameIndex(
                name: "IX_ListCard_ListId_Position",
                table: "ListCards",
                newName: "IX_ListCards_ListId_Position");

            migrationBuilder.RenameIndex(
                name: "IX_ListCard_BoardListId",
                table: "ListCards",
                newName: "IX_ListCards_BoardListId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ListCards",
                table: "ListCards",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ListCards_BoardLists_BoardListId",
                table: "ListCards",
                column: "BoardListId",
                principalTable: "BoardLists",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListCards_BoardLists_BoardListId",
                table: "ListCards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ListCards",
                table: "ListCards");

            migrationBuilder.RenameTable(
                name: "ListCards",
                newName: "ListCard");

            migrationBuilder.RenameIndex(
                name: "IX_ListCards_ListId_Position",
                table: "ListCard",
                newName: "IX_ListCard_ListId_Position");

            migrationBuilder.RenameIndex(
                name: "IX_ListCards_BoardListId",
                table: "ListCard",
                newName: "IX_ListCard_BoardListId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ListCard",
                table: "ListCard",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ListCard_BoardLists_BoardListId",
                table: "ListCard",
                column: "BoardListId",
                principalTable: "BoardLists",
                principalColumn: "Id");
        }
    }
}
