using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    [Migration("20260521170000_UseCoverPhotoIdForEventCoverImage")]
    public partial class UseCoverPhotoIdForEventCoverImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Events', 'CoverImageId') IS NOT NULL
                   AND COL_LENGTH('Events', 'CoverPhotoId') IS NOT NULL
                BEGIN
                    UPDATE [Events]
                    SET [CoverPhotoId] = [CoverImageId]
                    WHERE [CoverPhotoId] IS NULL
                      AND [CoverImageId] IS NOT NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID('FK_Events_CoverPhotos_CoverImageId', 'F') IS NOT NULL
                BEGIN
                    ALTER TABLE [Events] DROP CONSTRAINT [FK_Events_CoverPhotos_CoverImageId];
                END
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Events_CoverImageId'
                      AND object_id = OBJECT_ID('Events'))
                BEGIN
                    DROP INDEX [IX_Events_CoverImageId] ON [Events];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('Events', 'CoverImageId') IS NOT NULL
                BEGIN
                    ALTER TABLE [Events] DROP COLUMN [CoverImageId];
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Events_CoverPhotoId'
                      AND object_id = OBJECT_ID('Events'))
                BEGIN
                    CREATE INDEX [IX_Events_CoverPhotoId] ON [Events] ([CoverPhotoId]);
                END
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID('FK_Events_CoverPhotos_CoverPhotoId', 'F') IS NULL
                   AND OBJECT_ID('CoverPhotos', 'U') IS NOT NULL
                BEGIN
                    ALTER TABLE [Events]
                    ADD CONSTRAINT [FK_Events_CoverPhotos_CoverPhotoId]
                    FOREIGN KEY ([CoverPhotoId]) REFERENCES [CoverPhotos] ([Id])
                    ON DELETE SET NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID('FK_Events_CoverPhotos_CoverPhotoId', 'F') IS NOT NULL
                BEGIN
                    ALTER TABLE [Events] DROP CONSTRAINT [FK_Events_CoverPhotos_CoverPhotoId];
                END
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Events_CoverPhotoId'
                      AND object_id = OBJECT_ID('Events'))
                BEGIN
                    DROP INDEX [IX_Events_CoverPhotoId] ON [Events];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('Events', 'CoverImageId') IS NULL
                BEGIN
                    ALTER TABLE [Events] ADD [CoverImageId] uniqueidentifier NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('Events', 'CoverImageId') IS NOT NULL
                   AND COL_LENGTH('Events', 'CoverPhotoId') IS NOT NULL
                BEGIN
                    UPDATE [Events]
                    SET [CoverImageId] = [CoverPhotoId]
                    WHERE [CoverImageId] IS NULL
                      AND [CoverPhotoId] IS NOT NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Events_CoverImageId'
                      AND object_id = OBJECT_ID('Events'))
                BEGIN
                    CREATE INDEX [IX_Events_CoverImageId] ON [Events] ([CoverImageId]);
                END
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID('FK_Events_CoverPhotos_CoverImageId', 'F') IS NULL
                   AND OBJECT_ID('CoverPhotos', 'U') IS NOT NULL
                BEGIN
                    ALTER TABLE [Events]
                    ADD CONSTRAINT [FK_Events_CoverPhotos_CoverImageId]
                    FOREIGN KEY ([CoverImageId]) REFERENCES [CoverPhotos] ([Id]);
                END
                """);
        }
    }
}
