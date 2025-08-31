using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameListIdtoBoardListId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListCards_BoardLists_BoardListId",
                table: "ListCards");

            migrationBuilder.DropIndex(
                name: "IX_ListCards_BoardListId",
                table: "ListCards");

            migrationBuilder.DropIndex(
                name: "IX_ListCards_ListId_Position",
                table: "ListCards");

            migrationBuilder.DropColumn(
                name: "ListId",
                table: "ListCards");

            migrationBuilder.AlterColumn<Guid>(
                name: "BoardListId",
                table: "ListCards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListCards_BoardListId_Position",
                table: "ListCards",
                columns: new[] { "BoardListId", "Position" });

            migrationBuilder.AddForeignKey(
                name: "FK_ListCards_BoardLists_BoardListId",
                table: "ListCards",
                column: "BoardListId",
                principalTable: "BoardLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListCards_BoardLists_BoardListId",
                table: "ListCards");

            migrationBuilder.DropIndex(
                name: "IX_ListCards_BoardListId_Position",
                table: "ListCards");

            migrationBuilder.AlterColumn<Guid>(
                name: "BoardListId",
                table: "ListCards",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ListId",
                table: "ListCards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ListCards_BoardListId",
                table: "ListCards",
                column: "BoardListId");

            migrationBuilder.CreateIndex(
                name: "IX_ListCards_ListId_Position",
                table: "ListCards",
                columns: new[] { "ListId", "Position" });

            migrationBuilder.AddForeignKey(
                name: "FK_ListCards_BoardLists_BoardListId",
                table: "ListCards",
                column: "BoardListId",
                principalTable: "BoardLists",
                principalColumn: "Id");
        }
    }
}
