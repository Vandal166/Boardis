using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addpermissionbasedacforboardmembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "BoardMembers");

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "BoardMembers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "MemberPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    BoardMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "text", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberPermissions_BoardMembers_BoardId_BoardMemberId",
                        columns: x => new { x.BoardId, x.BoardMemberId },
                        principalTable: "BoardMembers",
                        principalColumns: new[] { "BoardId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Key" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"), "Owner" },
                    { new Guid("fedcba98-7654-3210-fedc-ba9876543210"), "Member" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardMembers_RoleId",
                table: "BoardMembers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberPermissions_BoardId_BoardMemberId",
                table: "MemberPermissions",
                columns: new[] { "BoardId", "BoardMemberId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BoardMembers_Roles_RoleId",
                table: "BoardMembers",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardMembers_Roles_RoleId",
                table: "BoardMembers");

            migrationBuilder.DropTable(
                name: "MemberPermissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_BoardMembers_RoleId",
                table: "BoardMembers");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "BoardMembers");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "BoardMembers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
