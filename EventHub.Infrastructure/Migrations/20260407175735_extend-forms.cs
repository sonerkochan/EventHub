using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class extendforms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("de300307-f24a-43b6-a6f5-f4f86f81f71c"));

            migrationBuilder.AddColumn<string>(
                name: "OrganizationName",
                table: "ApplicationForms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "ApplicationForms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "2144b981-2c5b-4d7c-9528-e7da68263fc3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "d028e465-409f-4142-8923-f92119f4b3f3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "f0dfa391-67a9-4fea-b90b-c90e2fec2b18");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "4682b576-dd37-40a1-8f4f-eb1bc1387ef7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "5ca77617-9971-4da3-b703-595c4680e088");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 7, 20, 57, 34, 426, DateTimeKind.Local).AddTicks(5259), "AQAAAAIAAYagAAAAEC+8N9jAcPcc+7AaohzcKSXQMzwe0lihGlHRHfehAiaCStKto5cOx7xCfKnx7NiZ6g==", new DateTime(2026, 4, 7, 20, 57, 34, 428, DateTimeKind.Local).AddTicks(8656) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("017328df-bca4-4e6b-b230-02570961d4dd"), 100L, new DateTime(2026, 4, 7, 17, 57, 34, 492, DateTimeKind.Utc).AddTicks(1407), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 4, 7, 17, 57, 34, 492, DateTimeKind.Utc).AddTicks(2027), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("017328df-bca4-4e6b-b230-02570961d4dd"));

            migrationBuilder.DropColumn(
                name: "OrganizationName",
                table: "ApplicationForms");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "ApplicationForms");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "05c9f7e9-8a20-4ab2-93e2-0781bd5bf660");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "dd260638-6596-45ed-af5c-5190687d67be");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "ecda54c0-9792-4947-a220-24fe7cc2f2d4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "91804734-fc84-46ee-8dbb-55eff1eb9d1e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "c42fe297-6fc0-4e00-8a29-2aa5a119b7dc");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 7, 20, 44, 45, 818, DateTimeKind.Local).AddTicks(6145), "AQAAAAIAAYagAAAAECEYSNDQNvYVd689GwYbCfo5qjvJ7ZYk/Ivo3Yu+AiXe1J7lVrBevj0ZHjCyXWCDew==", new DateTime(2026, 4, 7, 20, 44, 45, 820, DateTimeKind.Local).AddTicks(8367) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("de300307-f24a-43b6-a6f5-f4f86f81f71c"), 100L, new DateTime(2026, 4, 7, 17, 44, 45, 884, DateTimeKind.Utc).AddTicks(4151), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 4, 7, 17, 44, 45, 884, DateTimeKind.Utc).AddTicks(4868), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }
    }
}
