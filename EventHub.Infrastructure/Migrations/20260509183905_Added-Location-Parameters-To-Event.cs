using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedLocationParametersToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("408015af-51fb-4443-b2b1-5c5385f3567a"));

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "52ed0d55-82de-4402-b870-b89e8159a7a5");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "feb44200-eb46-42cc-883f-da17c016221b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "d8d24fb1-b725-44c1-9173-08f5a0b4e3cd");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "a32a6482-536d-42b1-aace-64c9a285c49b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "faa891aa-e28f-4a22-9b89-818ff453849f");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 9, 18, 39, 4, 536, DateTimeKind.Utc).AddTicks(4803), "AQAAAAIAAYagAAAAECroL/LvFgwM7xUvYLL+1R++am0/OcRTEAlK4XieJDB8gY1LISy+Yk50+VDizzKUeQ==", new DateTime(2026, 5, 9, 18, 39, 4, 536, DateTimeKind.Utc).AddTicks(4805) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("aa1d5ce2-000b-4f13-8b7e-58d4c7a5a215"), 100L, new DateTime(2026, 5, 9, 18, 39, 4, 585, DateTimeKind.Utc).AddTicks(873), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 5, 9, 18, 39, 4, 585, DateTimeKind.Utc).AddTicks(1245), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("aa1d5ce2-000b-4f13-8b7e-58d4c7a5a215"));

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Events");

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
                values: new object[] { new Guid("408015af-51fb-4443-b2b1-5c5385f3567a"), 100L, new DateTime(2026, 4, 22, 18, 11, 20, 239, DateTimeKind.Utc).AddTicks(4690), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 4, 19, 19, 45, 40, 215, DateTimeKind.Utc).AddTicks(6533), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }
    }
}
