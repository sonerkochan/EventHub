using EventHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260508144500_RepairServiceRentalRequestIdentity")]
    public partial class RepairServiceRentalRequestIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[ServiceRentalRequests]', N'U') IS NOT NULL
                   AND COLUMNPROPERTY(OBJECT_ID(N'[dbo].[ServiceRentalRequests]'), N'Id', 'IsIdentity') = 0
                BEGIN
                    IF OBJECT_ID(N'[dbo].[ServiceRentalRequests_Repair]', N'U') IS NOT NULL
                    BEGIN
                        DROP TABLE [dbo].[ServiceRentalRequests_Repair];
                    END

                    CREATE TABLE [dbo].[ServiceRentalRequests_Repair] (
                        [Id] int NOT NULL IDENTITY,
                        [SupplierServiceId] int NOT NULL,
                        [RequesterId] nvarchar(450) NOT NULL,
                        [Status] int NOT NULL,
                        [Message] nvarchar(max) NULL,
                        [ReviewedById] nvarchar(450) NULL,
                        [ResponseComment] nvarchar(max) NULL,
                        [RequestedAt] datetime2 NOT NULL,
                        [ReviewedAt] datetime2 NULL,
                        CONSTRAINT [PK_ServiceRentalRequests_Repair] PRIMARY KEY ([Id])
                    );

                    INSERT INTO [dbo].[ServiceRentalRequests_Repair]
                        ([SupplierServiceId], [RequesterId], [Status], [Message], [ReviewedById], [ResponseComment], [RequestedAt], [ReviewedAt])
                    SELECT
                        [SupplierServiceId],
                        [RequesterId],
                        [Status],
                        [Message],
                        [ReviewedById],
                        [ResponseComment],
                        [RequestedAt],
                        [ReviewedAt]
                    FROM [dbo].[ServiceRentalRequests]
                    WHERE [Id] IS NOT NULL
                      AND [SupplierServiceId] IS NOT NULL
                      AND [RequesterId] IS NOT NULL;

                    DROP TABLE [dbo].[ServiceRentalRequests];

                    EXEC sp_rename N'[dbo].[ServiceRentalRequests_Repair]', N'ServiceRentalRequests';
                    EXEC sp_rename N'[dbo].[PK_ServiceRentalRequests_Repair]', N'PK_ServiceRentalRequests';

                    ALTER TABLE [dbo].[ServiceRentalRequests]
                    ADD CONSTRAINT [FK_ServiceRentalRequests_AspNetUsers_RequesterId]
                        FOREIGN KEY ([RequesterId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;

                    ALTER TABLE [dbo].[ServiceRentalRequests]
                    ADD CONSTRAINT [FK_ServiceRentalRequests_AspNetUsers_ReviewedById]
                        FOREIGN KEY ([ReviewedById]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;

                    ALTER TABLE [dbo].[ServiceRentalRequests]
                    ADD CONSTRAINT [FK_ServiceRentalRequests_SupplierServices_SupplierServiceId]
                        FOREIGN KEY ([SupplierServiceId]) REFERENCES [dbo].[SupplierServices] ([Id]) ON DELETE NO ACTION;
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
        }
    }
}
