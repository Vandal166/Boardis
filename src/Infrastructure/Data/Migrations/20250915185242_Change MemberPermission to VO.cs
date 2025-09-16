using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMemberPermissiontoVO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberPermissions_BoardMembers_BoardId_BoardMemberId",
                table: "MemberPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberPermissions",
                table: "MemberPermissions");

            migrationBuilder.DropIndex(
                name: "IX_MemberPermissions_BoardId_BoardMemberId",
                table: "MemberPermissions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MemberPermissions");

            migrationBuilder.RenameColumn(
                name: "BoardMemberId",
                table: "MemberPermissions",
                newName: "UserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "GrantedAt",
                table: "MemberPermissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberPermissions",
                table: "MemberPermissions",
                columns: new[] { "BoardId", "UserId", "Permission" });

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPermissions_BoardMembers_BoardId_UserId",
                table: "MemberPermissions",
                columns: new[] { "BoardId", "UserId" },
                principalTable: "BoardMembers",
                principalColumns: new[] { "BoardId", "UserId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberPermissions_BoardMembers_BoardId_UserId",
                table: "MemberPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberPermissions",
                table: "MemberPermissions");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "MemberPermissions",
                newName: "BoardMemberId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "GrantedAt",
                table: "MemberPermissions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "MemberPermissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberPermissions",
                table: "MemberPermissions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MemberPermissions_BoardId_BoardMemberId",
                table: "MemberPermissions",
                columns: new[] { "BoardId", "BoardMemberId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MemberPermissions_BoardMembers_BoardId_BoardMemberId",
                table: "MemberPermissions",
                columns: new[] { "BoardId", "BoardMemberId" },
                principalTable: "BoardMembers",
                principalColumns: new[] { "BoardId", "UserId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
