using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthService.Migrations
{
    /// <inheritdoc />
    public partial class addCreatedByUpdatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MailCodes");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d5408263-3a12-4812-bf5b-c12b7c500cb0");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "a2011389-2df5-4a27-9e3c-1add9eb11d37", "11111111-1111-1111-1111-111111111111" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a2011389-2df5-4a27-9e3c-1add9eb11d37");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ConfirmationCode",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    RefreshCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChangeHistoryId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmationCode", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfirmationCode");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "MailCodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChangeHistoryId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RefreshCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailCodes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "a2011389-2df5-4a27-9e3c-1add9eb11d37", null, "Админ", "АДМИН" },
                    { "d5408263-3a12-4812-bf5b-c12b7c500cb0", null, "Пользователь", "ПОЛЬЗОВАТЕЛЬ" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "DeletedAt", "Email", "EmailConfirmed", "IsBlocked", "IsDeleted", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordExpiresAt", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UpdatedAt", "UserName" },
                values: new object[] { "11111111-1111-1111-1111-111111111111", 0, "eb789b56-5007-42c6-b9da-7d8b5062a1f1", new DateTimeOffset(new DateTime(2025, 9, 16, 11, 17, 31, 986, DateTimeKind.Unspecified).AddTicks(3101), new TimeSpan(0, 0, 0, 0, 0)), null, "admin@example.com", true, false, false, false, null, "ADMIN@EXAMPLE.COM", "ADMIN", new DateTimeOffset(new DateTime(2025, 12, 15, 11, 17, 31, 986, DateTimeKind.Unspecified).AddTicks(2771), new TimeSpan(0, 0, 0, 0, 0)), "AQAAAAIAAYagAAAAEERdUBT2upWECtSWExd5ger9eGVfUkzKqoRsS5npPBXtgn1ILZbYnEcmsCkov1t9Wg==", null, false, "32463bfe-ff0d-4953-afbe-e4d05633b2a7", true, new DateTimeOffset(new DateTime(2025, 9, 16, 11, 17, 31, 986, DateTimeKind.Unspecified).AddTicks(3101), new TimeSpan(0, 0, 0, 0, 0)), "admin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "a2011389-2df5-4a27-9e3c-1add9eb11d37", "11111111-1111-1111-1111-111111111111" });
        }
    }
}
