using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServersService.Migrations
{
    /// <inheritdoc />
    public partial class AddCPOdbSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelPermissionOverwrite_Channels_ChannelId",
                table: "ChannelPermissionOverwrite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChannelPermissionOverwrite",
                table: "ChannelPermissionOverwrite");

            migrationBuilder.RenameTable(
                name: "ChannelPermissionOverwrite",
                newName: "ChannelPermissionOverwrites");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChannelPermissionOverwrites",
                table: "ChannelPermissionOverwrites",
                columns: new[] { "ChannelId", "TargetId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelPermissionOverwrites_Channels_ChannelId",
                table: "ChannelPermissionOverwrites",
                column: "ChannelId",
                principalTable: "Channels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelPermissionOverwrites_Channels_ChannelId",
                table: "ChannelPermissionOverwrites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChannelPermissionOverwrites",
                table: "ChannelPermissionOverwrites");

            migrationBuilder.RenameTable(
                name: "ChannelPermissionOverwrites",
                newName: "ChannelPermissionOverwrite");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChannelPermissionOverwrite",
                table: "ChannelPermissionOverwrite",
                columns: new[] { "ChannelId", "TargetId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelPermissionOverwrite_Channels_ChannelId",
                table: "ChannelPermissionOverwrite",
                column: "ChannelId",
                principalTable: "Channels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
