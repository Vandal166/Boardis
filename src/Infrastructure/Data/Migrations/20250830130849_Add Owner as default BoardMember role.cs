using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerasdefaultBoardMemberrole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "BoardMembers",
                type: "text",
                nullable: false,
                defaultValue: "Owner",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Member");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "BoardMembers",
                type: "text",
                nullable: false,
                defaultValue: "Member",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Owner");
        }
    }
}
