using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addeventbaseprice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("77d5941d-575c-482a-a6b9-7ac623701c20"));

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Events",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "e32bee36-102c-4b06-a163-26f4226cce35");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "4dc1f43a-2cfc-41d0-9c7c-3a6df3d24f63");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "6912653a-7e9f-42da-a39d-a78219f971ba");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "964978d4-8bb0-4034-9a5e-334ccc1589b2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "c29336c1-793f-4522-b1bc-54c5d4c486d6");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 16, 18, 55, 46, 992, DateTimeKind.Local).AddTicks(5925), "AQAAAAIAAYagAAAAEHMfelf14EjalhfTXa952NrME2DOzavDgmTLzscquouKlVE5ddDBZLa6SJjIzGdplg==", new DateTime(2026, 4, 16, 18, 55, 46, 994, DateTimeKind.Local).AddTicks(2400) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("77319e29-3fe8-4892-ab10-337def59ac59"), 100L, new DateTime(2026, 4, 16, 15, 55, 47, 39, DateTimeKind.Utc).AddTicks(7594), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 4, 16, 15, 55, 47, 39, DateTimeKind.Utc).AddTicks(7952), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("77319e29-3fe8-4892-ab10-337def59ac59"));

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Events");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "5d8dc1e7-c19b-465e-9ee1-0b087401494f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "f713fc05-b3fb-4f87-a673-f147960b5afc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "3010433f-6573-479e-b96c-6c52275b88e5");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "63c98444-91f1-4251-8970-dcdf606f6d74");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "6349eae9-0dea-43d4-b301-77154f961f73");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 16, 18, 34, 0, 719, DateTimeKind.Local).AddTicks(7677), "AQAAAAIAAYagAAAAEDTduk6Duvn+yr8DqFb2JcCefq2+0G3mbmH6MGuqND4yUQ5Y2xzC7rRVTO/4sj154A==", new DateTime(2026, 4, 16, 18, 34, 0, 725, DateTimeKind.Local).AddTicks(5239) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("77d5941d-575c-482a-a6b9-7ac623701c20"), 100L, new DateTime(2026, 4, 16, 15, 34, 0, 847, DateTimeKind.Utc).AddTicks(2539), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 4, 16, 15, 34, 0, 847, DateTimeKind.Utc).AddTicks(4389), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }
    }
}
