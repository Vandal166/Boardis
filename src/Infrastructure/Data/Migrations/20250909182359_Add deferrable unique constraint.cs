using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Adddeferrableuniqueconstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_ListCard_BoardListId_Position"";

                ALTER TABLE ""ListCards""
                ADD CONSTRAINT ""IX_ListCard_BoardListId_Position""
                UNIQUE (""BoardListId"", ""Position"") DEFERRABLE INITIALLY DEFERRED;
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""ListCards""
                DROP CONSTRAINT IF EXISTS ""IX_ListCard_BoardListId_Position"";

                CREATE UNIQUE INDEX ""IX_ListCard_BoardListId_Position""
                ON ""ListCards"" (""BoardListId"", ""Position"");
            ");
        }
    }
}
