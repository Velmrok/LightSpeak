using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServersService.Migrations
{
    /// <inheritdoc />
    public partial class RefactorChannelStructureAndSplitItIntoDmAndServersChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ServerId",
                table: "Channels",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Channels",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Channels",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserHighId",
                table: "Channels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserLowId",
                table: "Channels",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Channels_UserLowId_UserHighId",
                table: "Channels",
                columns: new[] { "UserLowId", "UserHighId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Channels_UserLowId_UserHighId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "UserHighId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "UserLowId",
                table: "Channels");

            migrationBuilder.AlterColumn<string>(
                name: "ServerId",
                table: "Channels",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Channels",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
