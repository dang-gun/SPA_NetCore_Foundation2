using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGAuthServer_Sample_Cookie.Migrations.Mssql
{
    /// <summary>
    /// 
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="migrationBuilder"></param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    idUser = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SignName = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.idUser);
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "idUser", "PasswordHash", "SignName" },
                values: new object[] { 1, "1111", "test01" });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "idUser", "PasswordHash", "SignName" },
                values: new object[] { 2, "1111", "test02" });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="migrationBuilder"></param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
