using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServersService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeChannelPermissionOverwriteKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChannelPermissionOverwrite",
                table: "ChannelPermissionOverwrite");

            migrationBuilder.DropIndex(
                name: "IX_ChannelPermissionOverwrite_ChannelId",
                table: "ChannelPermissionOverwrite");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChannelPermissionOverwrite");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChannelPermissionOverwrite",
                table: "ChannelPermissionOverwrite",
                columns: new[] { "ChannelId", "TargetId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChannelPermissionOverwrite",
                table: "ChannelPermissionOverwrite");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "ChannelPermissionOverwrite",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChannelPermissionOverwrite",
                table: "ChannelPermissionOverwrite",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelPermissionOverwrite_ChannelId",
                table: "ChannelPermissionOverwrite",
                column: "ChannelId");
        }
    }
}
