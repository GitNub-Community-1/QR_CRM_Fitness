using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestQRApp.Migrations
{
    /// <inheritdoc />
    public partial class Ыусщтв : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Staffs",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "PasswordHash", "Role" },
                values: new object[] { "AQAAAAIAAYagAAAAEJ5dMv3Q8/uUeSjRY1l4zGZpT6mXnUvW1Y2Z3Q4R5t6y7u8i9o==", "Admin" });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "PasswordHash", "Role" },
                values: new object[] { "AQAAAAIAAYagAAAAIModMv3Q8/uUeSjRY1l4zGZpT6mXnUvW1Y2Z3Q4R5t6y7u8i9o==", "Moderator" });

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_Login",
                table: "Staffs",
                column: "Login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Staffs_Login",
                table: "Staffs");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Staffs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "PasswordHash", "Role" },
                values: new object[] { "AQAAAAIAAYagAAAAEHl3hVZ06QbNBAAbImfiSNlu4lSPOsbUcOqOkB8Tygorhlwm6akgTud6wTjQU5fxLA==", 0 });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "PasswordHash", "Role" },
                values: new object[] { "AQAAAAIAAYagAAAAELc/H8vKERjMm4Llwl8UPJTYMPBd9cu7HPxb3N/Mbfnyj3RbMsGGQcGclfgUKi0czA==", 1 });
        }
    }
}
