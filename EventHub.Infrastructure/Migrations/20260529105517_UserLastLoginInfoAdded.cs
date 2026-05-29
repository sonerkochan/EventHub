using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserLastLoginInfoAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("342df402-06ac-49d5-923b-65eec89900d0"));

            migrationBuilder.AddColumn<string>(
                name: "LastLoginDevice",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastLoginIP",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOnline",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "7deac7e2-8225-40c3-8a6e-c6480ceff30a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "1caded3c-b521-4de5-8842-df966af45be3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "8566a253-7a5d-434a-93f7-f9e30e4c851b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "681d155b-51a5-480e-9f27-42dcb3ab7015");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "0ecf719b-1baf-4b95-9ae6-109b2c3058bf");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "LastLoginDevice", "LastLoginIP", "LastOnline", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 29, 10, 55, 8, 27, DateTimeKind.Utc).AddTicks(1987), null, null, null, "AQAAAAIAAYagAAAAEG38AuyGuEieGSgIjw2GqBwsTbF345G+3m8+MGVgIE5WOd/6dwXLH196K/iTJNEZxA==", new DateTime(2026, 5, 29, 10, 55, 8, 27, DateTimeKind.Utc).AddTicks(1993) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("3190929a-5292-4dc3-8fd1-5adf73d8982a"), 100L, new DateTime(2026, 5, 29, 10, 55, 8, 96, DateTimeKind.Utc).AddTicks(7462), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 5, 29, 10, 55, 8, 96, DateTimeKind.Utc).AddTicks(8240), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("3190929a-5292-4dc3-8fd1-5adf73d8982a"));

            migrationBuilder.DropColumn(
                name: "LastLoginDevice",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginIP",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastOnline",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "6656805f-5ad3-4fff-b40b-85111367fa42");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "37938732-5e72-4d0c-bece-3dd0d2d9df63");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "c32cf7e0-8731-4470-9dd3-4134b52b3b2f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "6917c367-e4d0-4681-856e-18e6fe4f4f66");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "5e670fba-2eeb-4097-bb73-b528dae75128");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 22, 8, 53, 31, 909, DateTimeKind.Utc).AddTicks(4507), "AQAAAAIAAYagAAAAEOKd4lDZN14LEkTIe62tB+66BjxxWcSn0LShknRCIm9ncXzE1UbSdMpeBEDjtnimSg==", new DateTime(2026, 5, 22, 8, 53, 31, 909, DateTimeKind.Utc).AddTicks(4510) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("342df402-06ac-49d5-923b-65eec89900d0"), 100L, new DateTime(2026, 5, 22, 8, 53, 31, 953, DateTimeKind.Utc).AddTicks(7913), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 5, 22, 8, 53, 31, 953, DateTimeKind.Utc).AddTicks(8270), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }
    }
}
