using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AnalyticsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("8792ba8e-85ed-4b73-b5d2-00c97a8fe038"));

            migrationBuilder.CreateTable(
                name: "AnalyticsLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referrer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsLogs", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsLogs");

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("342df402-06ac-49d5-923b-65eec89900d0"));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "e81aa176-ec5f-4e94-9a5d-6feea4c1e187");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "afea8c3e-3a43-40d8-a67b-7fe85c47d2db");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "bf07db76-9646-4de5-a38e-4f86d2860455");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "0931cbbf-f700-4719-b3c8-ba1cc5e83639");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "87c12746-5e05-42d9-8ca4-102de958ca4c");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 21, 14, 33, 28, 130, DateTimeKind.Utc).AddTicks(2199), "AQAAAAIAAYagAAAAEDdZkDW0RD/t9FMGpTIsATCMEqFFKerMYRlvFG9Tgqdb3m/r9vfuAIMH9yilaax7gA==", new DateTime(2026, 5, 21, 14, 33, 28, 130, DateTimeKind.Utc).AddTicks(2202) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("8792ba8e-85ed-4b73-b5d2-00c97a8fe038"), 100L, new DateTime(2026, 5, 21, 14, 33, 28, 170, DateTimeKind.Utc).AddTicks(9262), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 5, 21, 14, 33, 28, 170, DateTimeKind.Utc).AddTicks(9622), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }
    }
}
