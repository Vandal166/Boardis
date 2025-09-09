using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueboardlistidandpositionindex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ListCards_BoardListId_Position",
                table: "ListCards");

            migrationBuilder.CreateIndex(
                name: "IX_ListCard_BoardListId_Position",
                table: "ListCards",
                columns: new[] { "BoardListId", "Position" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ListCard_BoardListId_Position",
                table: "ListCards");

            migrationBuilder.CreateIndex(
                name: "IX_ListCards_BoardListId_Position",
                table: "ListCards",
                columns: new[] { "BoardListId", "Position" });
        }
    }
}
