using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedPhotoModelToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("e2234787-cb89-4bf0-8662-9a82d4609a27"));

            migrationBuilder.AddColumn<Guid>(
                name: "CoverImageId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CoverPhotoId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CoverPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverPhotos", x => x.Id);
                });

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
                columns: new[] { "CreatedAt", "IsActive", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 21, 14, 33, 28, 130, DateTimeKind.Utc).AddTicks(2199), true, "AQAAAAIAAYagAAAAEDdZkDW0RD/t9FMGpTIsATCMEqFFKerMYRlvFG9Tgqdb3m/r9vfuAIMH9yilaax7gA==", new DateTime(2026, 5, 21, 14, 33, 28, 130, DateTimeKind.Utc).AddTicks(2202) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("8792ba8e-85ed-4b73-b5d2-00c97a8fe038"), 100L, new DateTime(2026, 5, 21, 14, 33, 28, 170, DateTimeKind.Utc).AddTicks(9262), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 5, 21, 14, 33, 28, 170, DateTimeKind.Utc).AddTicks(9622), new Guid("12345678-90ab-cdef-1234-567890abcdef") });

            migrationBuilder.CreateIndex(
                name: "IX_Events_CoverImageId",
                table: "Events",
                column: "CoverImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_CoverPhotos_CoverImageId",
                table: "Events",
                column: "CoverImageId",
                principalTable: "CoverPhotos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_CoverPhotos_CoverImageId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "CoverPhotos");

            migrationBuilder.DropIndex(
                name: "IX_Events_CoverImageId",
                table: "Events");

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("8792ba8e-85ed-4b73-b5d2-00c97a8fe038"));

            migrationBuilder.DropColumn(
                name: "CoverImageId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CoverPhotoId",
                table: "Events");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07358494-247c-421c-8f7f-82c12be55276",
                column: "ConcurrencyStamp",
                value: "71b5daad-cd76-476d-9885-0c9e92bfd669");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b2c3d4e5-f6a7-8901-bcde-f01234567891",
                column: "ConcurrencyStamp",
                value: "d33edf03-d801-44d4-bf4b-2d47cdaf7ad5");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d4e5f6-a7b8-9012-cdef-012345678912",
                column: "ConcurrencyStamp",
                value: "ab53b537-8466-44b0-9f4a-9e5ff6bda3f7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d9de7285-b674-454c-9889-5210abb8d347",
                column: "ConcurrencyStamp",
                value: "128ce202-7617-4945-9dc8-681a6db17252");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-0123-def0-123456789abc",
                column: "ConcurrencyStamp",
                value: "47335ec4-9392-4a2b-ae99-7c39862d61d1");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                columns: new[] { "CreatedAt", "IsActive", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 8, 14, 25, 27, 144, DateTimeKind.Utc).AddTicks(6465), false, "AQAAAAIAAYagAAAAECc6Z/QFZ2Vl1kh/e4DhvMUDmK972iLpoAgyhKfOJsOQ/0Cr12lfhS15Y4eqg+BsZQ==", new DateTime(2026, 5, 8, 14, 25, 27, 144, DateTimeKind.Utc).AddTicks(6470) });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "Capacity", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "RoomType", "UpdatedAt", "VenueId" },
                values: new object[] { new Guid("e2234787-cb89-4bf0-8662-9a82d4609a27"), 100L, new DateTime(2026, 5, 8, 14, 25, 27, 193, DateTimeKind.Utc).AddTicks(8934), new Guid("f7a1b2c3-d4e5-6789-abcd-ef0123456789"), "Very nice and cool big room (to test)", true, "Fancy", 0, new DateTime(2026, 5, 8, 14, 25, 27, 193, DateTimeKind.Utc).AddTicks(9310), new Guid("12345678-90ab-cdef-1234-567890abcdef") });
        }
    }
}
