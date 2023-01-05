using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YAMB.Migrations
{
    public partial class Add_UserBans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserBans",
                columns: table => new
                {
                    BanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    BannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnbannedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("UserBans_pk", x => x.BanId);
                });

            migrationBuilder.CreateIndex(
                name: "UserBans_BanId_uindex",
                table: "UserBans",
                column: "BanId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBans");
        }
    }
}
