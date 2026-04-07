using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedapplicationforms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("2e75dec4-b492-4ef0-8b4a-792f93cf9175"));

            migrationBuilder.CreateTable(
                name: "ApplicationForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReviewComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationForms_AspNetUsers_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationForms_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationForms_ReviewedById",
                table: "ApplicationForms",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationForms_UserId",
                table: "ApplicationForms",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationForms");

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: new Guid("de300307-f24a-43b6-a6f5-f4f86f81f71c"));

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
    }
}
