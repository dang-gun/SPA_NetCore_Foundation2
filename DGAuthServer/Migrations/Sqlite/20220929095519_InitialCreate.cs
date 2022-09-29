using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGAuthServer.Migrations.Sqlite
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DGAuthServer_AccessToken",
                columns: table => new
                {
                    idDgAuthAccessToken = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    idUser = table.Column<long>(type: "INTEGER", nullable: false),
                    Class = table.Column<string>(type: "TEXT", nullable: false),
                    Secret = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DGAuthServer_AccessToken", x => x.idDgAuthAccessToken);
                });

            migrationBuilder.CreateTable(
                name: "DGAuthServer_RefreshToken",
                columns: table => new
                {
                    idDgAuthRefreshToken = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    idUser = table.Column<long>(type: "INTEGER", nullable: false),
                    Class = table.Column<string>(type: "TEXT", nullable: false),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: false),
                    GenerateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RevokeTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IpCreated = table.Column<string>(type: "TEXT", nullable: true),
                    ExpiredIs = table.Column<bool>(type: "INTEGER", nullable: false),
                    RevokeIs = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActiveIs = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DGAuthServer_RefreshToken", x => x.idDgAuthRefreshToken);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DGAuthServer_AccessToken");

            migrationBuilder.DropTable(
                name: "DGAuthServer_RefreshToken");
        }
    }
}
