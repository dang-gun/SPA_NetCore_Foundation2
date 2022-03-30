using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    public partial class AddUser_Test03 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordReset", "Verified" },
                values: new object[] { "$2a$11$f3jPHRNyc4A4n8yxbUT1eOYYkhQbLpNyiGoj.VcpqLsj86WUr9hGm", new DateTime(2022, 3, 30, 10, 46, 18, 263, DateTimeKind.Local).AddTicks(9978), new DateTime(2022, 3, 30, 10, 46, 18, 263, DateTimeKind.Local).AddTicks(9966) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordReset", "Verified" },
                values: new object[] { "$2a$11$AEd56hYKkdaj9PhzdE9GDutcfuI57cy0rrU0dtQ6ve01UGjhmtzry", null, null });
        }
    }
}
