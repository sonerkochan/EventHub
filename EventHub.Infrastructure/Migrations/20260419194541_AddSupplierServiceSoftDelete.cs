using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierServiceSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("6b1cb3ac-4206-4dca-a23e-1e4aef1c34e5"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SupplierServices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SupplierServices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SupplierServices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "d4e08e2b-1541-44b5-b8fd-106b2de74cf2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "77178a42-41fb-4afa-8e85-ffe3f90ce9e8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "4b86fed7-2cdf-4a58-a54d-9ea4de6721f6");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "5c1069b7-d61d-4598-a25c-9604c6799101");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "3fd95861-2dd0-484e-ab05-e1904ea04c5b");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 19, 22, 45, 40, 169, DateTimeKind.Local).AddTicks(2699), "AQAAAAIAAYagAAAAEJKsEPaQz1urxLqUQLcMxuoBrYC5HLKnUiU26PHynPy3lU0W1xn5KuU0MOXrDzgYYw==", new DateTime(2026, 4, 19, 22, 45, 40, 170, DateTimeKind.Local).AddTicks(7784) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("7236f520-d1e9-415c-9fe8-6d02c518c63e"), 100L, new DateTime(2026, 4, 19, 19, 45, 40, 215, DateTimeKind.Utc).AddTicks(6151), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 4, 19, 19, 45, 40, 215, DateTimeKind.Utc).AddTicks(6533), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("7236f520-d1e9-415c-9fe8-6d02c518c63e"));

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SupplierServices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SupplierServices");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SupplierServices");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "c83fe32c-9b92-41ed-b549-931124cc3927");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "99230de4-0ca7-4f7b-848d-80e2384ea683");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "9301a857-eeff-440a-a08b-cae104bce5d7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "27cc526f-a37c-4c5e-9235-f82ad3dd35c9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "b783f9a0-2c2c-4925-92ed-924ea7f94b11");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 4, 19, 18, 40, 33, 460, DateTimeKind.Local).AddTicks(7256), "AQAAAAIAAYagAAAAELS3+7pYMTT/vSXNKBse7r4kigifwJ+9cyeQFWPyywZh/afcGhhFckejCf0/k7R4uQ==", new DateTime(2026, 4, 19, 18, 40, 33, 462, DateTimeKind.Local).AddTicks(2842) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("6b1cb3ac-4206-4dca-a23e-1e4aef1c34e5"), 100L, new DateTime(2026, 4, 19, 15, 40, 33, 508, DateTimeKind.Utc).AddTicks(728), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 4, 19, 15, 40, 33, 508, DateTimeKind.Utc).AddTicks(1090), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }
    }
}
