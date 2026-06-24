using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Infrastructure.Data.Configuration
{
    internal class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.HasData(CreateRoom());
        }

        private List<Room> CreateRoom()
        {
            return new List<Room>()
            {
                new Room()
                {
                    RoomId = Guid.Parse("3190929a-5292-4dc3-8fd1-5adf73d8982a"),
                    Name = "Fancy",
                    Description = "Very nice and cool big room (to test)",
                    Capacity = 100,
                    CreatedAt = new DateTime(2026, 5, 29, 10, 55, 8, 96, DateTimeKind.Utc).AddTicks(7462),
                    CreatedBy = Guid.Parse("f7a1b2c3-d4e5-6789-abcd-ef0123456789"),
                    UpdatedAt = new DateTime(2026, 5, 29, 10, 55, 8, 96, DateTimeKind.Utc).AddTicks(8240),
                    IsActive = true,
                    VenueId = Guid.Parse("12345678-90ab-cdef-1234-567890abcdef"),
                    RoomType = RoomType.Theatre
                }
            };
        }
    }
}
