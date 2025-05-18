using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class seedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { 1, 0, "11111111-1111-1111-1111-111111111111", "test@example1.com", false, false, null, "TEST@EXAMPLE1.COM", "TEST1", "AQAAAAIAAYagAAAAEDel75C1UyO7+ftDht11k+LnMnur1SO7cMUYgZCDNsBanZE2Nl4Kd3xtTyBS6mY+Hg==", null, false, "22222222-2222-2222-2222-222222222222", false, "test1" },
                    { 2, 0, "33333333-3333-3333-3333-333333333333", "test@example2.com", false, false, null, "TEST@EXAMPLE2.COM", "TEST2", "AQAAAAIAAYagAAAAEJ/mP72fwry5hglb7ubp1kd7e++gc6+fD5nI0p/smOIe0daLcSjDQ7UY4PyO+ocp4Q==", null, false, "44444444-4444-4444-4444-444444444444", false, "test2" }
                });

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "Id", "AnonId", "CreatedAt", "Nickname", "UserId" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "TestPlayer1", 1 },
                    { 2, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "TestPlayer2", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
