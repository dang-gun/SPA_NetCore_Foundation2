using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    public partial class AddUser_Test02 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "PasswordHash" },
                values: new object[] { "test01@a.net", "$2a$11$AEd56hYKkdaj9PhzdE9GDutcfuI57cy0rrU0dtQ6ve01UGjhmtzry" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "PasswordHash" },
                values: new object[] { "test01", "$2a$11$KYds.wElAPUjtcofirh3UOMU/XUKTFYZjnJMPp9FsB/HkyADwB2zy" });
        }
    }
}
