using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Removerowversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "ListCards");
            
            migrationBuilder.Sql(@"
                ALTER TABLE ""ListCards""
                DROP CONSTRAINT IF EXISTS ""IX_ListCard_BoardListId_Position"";

                CREATE UNIQUE INDEX ""IX_ListCard_BoardListId_Position""
                ON ""ListCards"" (""BoardListId"", ""Position"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "ListCards",
                type: "bytea",
                rowVersion: true,
                nullable: true);
        }
    }
}
