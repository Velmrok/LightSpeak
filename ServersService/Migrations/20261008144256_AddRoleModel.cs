using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServersService.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Permissions = table.Column<long>(type: "bigint", nullable: false),
                    MemberServerId = table.Column<string>(type: "text", nullable: true),
                    MemberUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role_Members_MemberUserId_MemberServerId",
                        columns: x => new { x.MemberUserId, x.MemberServerId },
                        principalTable: "Members",
                        principalColumns: new[] { "UserId", "ServerId" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Role_MemberUserId_MemberServerId",
                table: "Role",
                columns: new[] { "MemberUserId", "MemberServerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
