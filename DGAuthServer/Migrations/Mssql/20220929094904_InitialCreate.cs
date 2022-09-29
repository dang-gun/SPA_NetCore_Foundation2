using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGAuthServer.Migrations.Mssql
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DGAuthServer_AccessToken",
                columns: table => new
                {
                    idDgAuthAccessToken = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idUser = table.Column<long>(type: "bigint", nullable: false),
                    Class = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DGAuthServer_AccessToken", x => x.idDgAuthAccessToken);
                });

            migrationBuilder.CreateTable(
                name: "DGAuthServer_RefreshToken",
                columns: table => new
                {
                    idDgAuthRefreshToken = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idUser = table.Column<long>(type: "bigint", nullable: false),
                    Class = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GenerateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokeTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IpCreated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiredIs = table.Column<bool>(type: "bit", nullable: false),
                    RevokeIs = table.Column<bool>(type: "bit", nullable: false),
                    ActiveIs = table.Column<bool>(type: "bit", nullable: false)
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
