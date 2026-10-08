using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServersService.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesInServerModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Role_Members_MemberUserId_MemberServerId",
                table: "Role");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Role",
                table: "Role");

            migrationBuilder.RenameTable(
                name: "Role",
                newName: "Roles");

            migrationBuilder.RenameIndex(
                name: "IX_Role_MemberUserId_MemberServerId",
                table: "Roles",
                newName: "IX_Roles_MemberUserId_MemberServerId");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Roles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ServerId",
                table: "Roles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ChannelPermissionOverwrite",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ChannelId = table.Column<string>(type: "text", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<string>(type: "text", nullable: false),
                    Allow = table.Column<long>(type: "bigint", nullable: false),
                    Deny = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelPermissionOverwrite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelPermissionOverwrite_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ServerId",
                table: "Roles",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelPermissionOverwrite_ChannelId",
                table: "ChannelPermissionOverwrite",
                column: "ChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Members_MemberUserId_MemberServerId",
                table: "Roles",
                columns: new[] { "MemberUserId", "MemberServerId" },
                principalTable: "Members",
                principalColumns: new[] { "UserId", "ServerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Servers_ServerId",
                table: "Roles",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Members_MemberUserId_MemberServerId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Servers_ServerId",
                table: "Roles");

            migrationBuilder.DropTable(
                name: "ChannelPermissionOverwrite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_ServerId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ServerId",
                table: "Roles");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Role");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_MemberUserId_MemberServerId",
                table: "Role",
                newName: "IX_Role_MemberUserId_MemberServerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Role",
                table: "Role",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Role_Members_MemberUserId_MemberServerId",
                table: "Role",
                columns: new[] { "MemberUserId", "MemberServerId" },
                principalTable: "Members",
                principalColumns: new[] { "UserId", "ServerId" });
        }
    }
}
