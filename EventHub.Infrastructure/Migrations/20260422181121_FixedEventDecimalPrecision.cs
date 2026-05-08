using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedEventDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("ce7f5181-c792-4c7b-8c1d-2ef1d618f52d"));

            migrationBuilder.AlterColumn<decimal>(
                name: "BasePrice",
                table: "Events",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "65cd56eb-f0b4-4e2e-a9a3-2fa73fd42dee");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "7e872099-1480-4fc1-ba54-da77d881b99a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "b8cc85c8-2a45-4a08-ad66-d5fe6eee52d6");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "cd8cee83-5eb6-46fd-98e5-7b154205a70a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "5651dfc8-721e-4b5e-9959-64d4f35325f9");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 22, 18, 11, 20, 178, DateTimeKind.Utc).AddTicks(339), "AQAAAAIAAYagAAAAED8VzymxLO/vGOtOa/Nd1HPEctnm6pVHUGKDKtMS+PjFd1fqKJYORxTpb99gUuxngQ==", new DateTime(2026, 4, 22, 18, 11, 20, 178, DateTimeKind.Utc).AddTicks(343) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("408015af-51fb-4443-b2b1-5c5385f3567a"), 100L, new DateTime(2026, 4, 22, 18, 11, 20, 239, DateTimeKind.Utc).AddTicks(4690), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 4, 22, 18, 11, 20, 239, DateTimeKind.Utc).AddTicks(5519), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("408015af-51fb-4443-b2b1-5c5385f3567a"));

            migrationBuilder.AlterColumn<decimal>(
                name: "BasePrice",
                table: "Events",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "c332fd76-fc43-4496-96d6-887eeeb1d199");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "3e803610-90ad-4366-9a47-804130cb7d18");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "86e17591-4470-4da2-94f8-eec826799f55");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "be03a2e4-2247-46a0-8fd2-65f4b91a4b38");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "9bc23b6c-9f2b-4f82-aaf3-163010445831");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 22, 17, 11, 54, 464, DateTimeKind.Utc).AddTicks(1759), "AQAAAAIAAYagAAAAEAWQDnbOwkLJUtbMgGb++cWc+owDJVolILGF/Amy0NHNocg7ZlJBlpur3bu8N2GXrA==", new DateTime(2026, 4, 22, 17, 11, 54, 464, DateTimeKind.Utc).AddTicks(1763) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("ce7f5181-c792-4c7b-8c1d-2ef1d618f52d"), 100L, new DateTime(2026, 4, 22, 17, 11, 54, 541, DateTimeKind.Utc).AddTicks(2183), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 4, 22, 17, 11, 54, 541, DateTimeKind.Utc).AddTicks(3001), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }
    }
}
