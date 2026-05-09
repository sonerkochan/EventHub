using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceRentalRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[ServiceRentalRequests]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[ServiceRentalRequests] (
                        [Id] int NOT NULL IDENTITY,
                        [SupplierServiceId] int NOT NULL,
                        [RequesterId] nvarchar(450) NOT NULL,
                        [Status] int NOT NULL,
                        [Message] nvarchar(max) NULL,
                        [ReviewedById] nvarchar(450) NULL,
                        [ResponseComment] nvarchar(max) NULL,
                        [RequestedAt] datetime2 NOT NULL,
                        [ReviewedAt] datetime2 NULL,
                        CONSTRAINT [PK_ServiceRentalRequests] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ServiceRentalRequests_AspNetUsers_RequesterId] FOREIGN KEY ([RequesterId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_ServiceRentalRequests_AspNetUsers_ReviewedById] FOREIGN KEY ([ReviewedById]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_ServiceRentalRequests_SupplierServices_SupplierServiceId] FOREIGN KEY ([SupplierServiceId]) REFERENCES [dbo].[SupplierServices] ([Id]) ON DELETE NO ACTION
                    );
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[ServiceRentalRequests]', N'SupplierServiceId') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[ServiceRentalRequests] ADD [SupplierServiceId] int NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[ServiceRentalRequests]', N'RequesterId') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[ServiceRentalRequests] ADD [RequesterId] nvarchar(450) NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[ServiceRentalRequests]', N'Status') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[ServiceRentalRequests] ADD [Status] int NOT NULL CONSTRAINT [DF_ServiceRentalRequests_Status] DEFAULT 1;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[ServiceRentalRequests]', N'Message') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[ServiceRentalRequests] ADD [Message] nvarchar(max) NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[ServiceRentalRequests]', N'ReviewedById') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[ServiceRentalRequests] ADD [ReviewedById] nvarchar(450) NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[ServiceRentalRequests]', N'ResponseComment') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[ServiceRentalRequests] ADD [ResponseComment] nvarchar(max) NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[ServiceRentalRequests]', N'RequestedAt') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[ServiceRentalRequests] ADD [RequestedAt] datetime2 NOT NULL CONSTRAINT [DF_ServiceRentalRequests_RequestedAt] DEFAULT SYSUTCDATETIME();
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[ServiceRentalRequests]', N'ReviewedAt') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[ServiceRentalRequests] ADD [ReviewedAt] datetime2 NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_ServiceRentalRequests_RequesterId'
                    AND object_id = OBJECT_ID(N'[dbo].[ServiceRentalRequests]')
                )
                BEGIN
                    CREATE INDEX [IX_ServiceRentalRequests_RequesterId]
                    ON [dbo].[ServiceRentalRequests] ([RequesterId]);
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_ServiceRentalRequests_ReviewedById'
                    AND object_id = OBJECT_ID(N'[dbo].[ServiceRentalRequests]')
                )
                BEGIN
                    CREATE INDEX [IX_ServiceRentalRequests_ReviewedById]
                    ON [dbo].[ServiceRentalRequests] ([ReviewedById]);
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_ServiceRentalRequests_SupplierServiceId'
                    AND object_id = OBJECT_ID(N'[dbo].[ServiceRentalRequests]')
                )
                BEGIN
                    CREATE INDEX [IX_ServiceRentalRequests_SupplierServiceId]
                    ON [dbo].[ServiceRentalRequests] ([SupplierServiceId]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRentalRequests");
        }
    }
}
