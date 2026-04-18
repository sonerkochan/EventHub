using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatepaymentstripefields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("017328df-bca4-4e6b-b230-02570961d4dd"));

            migrationBuilder.AlterColumn<string>(
                name: "StripeSessionId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StripePaymentIntentId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("77d5941d-575c-482a-a6b9-7ac623701c20"));

            migrationBuilder.AlterColumn<Guid>(
                name: "StripeSessionId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StripePaymentIntentId",
                table: "Payments",
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
    }
}
