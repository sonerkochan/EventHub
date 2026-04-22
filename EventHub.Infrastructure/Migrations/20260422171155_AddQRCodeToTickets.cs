using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQRCodeToTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("77319e29-3fe8-4892-ab10-337def59ac59"));

            migrationBuilder.AddColumn<string>(
                name: "QRCodeImage",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripeRefundId",
                table: "Refunds",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("ce7f5181-c792-4c7b-8c1d-2ef1d618f52d"));

            migrationBuilder.DropColumn(
                name: "QRCodeImage",
                table: "Tickets");

            migrationBuilder.AlterColumn<Guid>(
                name: "StripeRefundId",
                table: "Refunds",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "a91877c7-0e3d-491c-b75c-c8397f80bfbd");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "5d253c9f-5311-4d9c-b504-90f3cbb294e8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "b7da1f51-edb1-4792-9e83-d55ed1e6b1b7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "ad7d29b6-ad21-4df3-b3ec-d2772f5309ad");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "f90f14e4-0713-477d-b416-b1576e00d523");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 8, 0, 2, 10, 349, DateTimeKind.Local).AddTicks(5823), "AQAAAAIAAYagAAAAEMkExl/+jH/vScxWXSpJ3POuVEDPfMJMqC4S1DpO7IFttQP0MaIgypAby1BFd5pB/Q==", new DateTime(2026, 4, 8, 0, 2, 10, 351, DateTimeKind.Local).AddTicks(8354) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("77319e29-3fe8-4892-ab10-337def59ac59"), 100L, new DateTime(2026, 4, 7, 21, 2, 10, 410, DateTimeKind.Utc).AddTicks(7551), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 4, 16, 15, 55, 47, 39, DateTimeKind.Utc).AddTicks(7952), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }
    }
}
