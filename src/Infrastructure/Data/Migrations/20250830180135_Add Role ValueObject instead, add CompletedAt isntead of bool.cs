using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleValueObjectinsteadaddCompletedAtisnteadofbool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "ListCards");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "ListCards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "BoardMembers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Owner");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "ListCards");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "ListCards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "BoardMembers",
                type: "text",
                nullable: false,
                defaultValue: "Owner",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
