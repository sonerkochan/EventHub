using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModelScaffoldingMosltyComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("8b1b4ff6-b6e4-4d27-ae14-4cfd161dd200"));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "df10a873-c5ed-457c-8268-ead15c7e9b3f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "e7051243-29ac-4184-8232-e1bbce5acd6d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "736b5289-bff0-4b67-90ec-f6854bc1ab3b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "04179eb3-1487-464a-a37a-f5ddf71334f6");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "16c4aa31-7eb1-4921-8f98-0abd81b3adca");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 28, 16, 32, 39, 260, DateTimeKind.Local).AddTicks(6081), "AQAAAAIAAYagAAAAEFapYWxrL6GbElBdHhFjK6/d5wiimKJvBlTeP8hngn3MiS8pOllDgD2nP16CL1Z+gQ==", new DateTime(2026, 3, 28, 16, 32, 39, 262, DateTimeKind.Local).AddTicks(5090) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("2e75dec4-b492-4ef0-8b4a-792f93cf9175"), 100L, new DateTime(2026, 3, 28, 14, 32, 39, 325, DateTimeKind.Utc).AddTicks(6036), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 3, 28, 14, 32, 39, 325, DateTimeKind.Utc).AddTicks(6877), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("2e75dec4-b492-4ef0-8b4a-792f93cf9175"));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "21a600b0-2e81-44d6-af44-b5a2a11be2fb");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "05a6b94f-aa08-492b-b1d6-8fda04d8ca45");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "5a7bee3c-3621-4f75-9cb9-579e1bb6447f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "06a76a27-5e1d-45e1-afc1-b27b73bb1975");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "a47fddf1-2fec-4fa3-95e0-ec6f066dcf33");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 21, 10, 31, 59, 938, DateTimeKind.Local).AddTicks(7092), "AQAAAAIAAYagAAAAED91jcBKU5swQ4/5w29+fnIEkj5wkMtAj39J7Oa2Z7giFFEuQzE2sUuSQAAKmBZtuQ==", new DateTime(2026, 3, 21, 10, 31, 59, 940, DateTimeKind.Local).AddTicks(8118) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("8b1b4ff6-b6e4-4d27-ae14-4cfd161dd200"), 100L, new DateTime(2026, 3, 21, 8, 32, 0, 34, DateTimeKind.Utc).AddTicks(6191), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 3, 21, 8, 32, 0, 34, DateTimeKind.Utc).AddTicks(7259), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }
    }
}
