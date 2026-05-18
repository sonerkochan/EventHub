using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertEventCoordinatesToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Events]
                SET [Longitude] = '0'
                WHERE [Longitude] IS NULL
                    OR LTRIM(RTRIM([Longitude])) = ''
                    OR TRY_CONVERT(decimal(10,7), [Longitude]) IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE [Events]
                SET [Latitude] = '0'
                WHERE [Latitude] IS NULL
                    OR LTRIM(RTRIM([Latitude])) = ''
                    OR TRY_CONVERT(decimal(10,7), [Latitude]) IS NULL;
                """);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Events",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Events",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Longitude",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)",
                oldPrecision: 10,
                oldScale: 7);

            migrationBuilder.AlterColumn<string>(
                name: "Latitude",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)",
                oldPrecision: 10,
                oldScale: 7);
        }
    }
}
