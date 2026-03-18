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
                    RoomId = Guid.NewGuid(),
                    Name = "Fancy",
                    Description = "Very nice and cool big room (to test)",
                    Capacity = 100,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Parse("f7a1b2c3-d4e5-6789-abcd-ef0123456789"),
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    VenueId = Guid.Parse("12345678-90ab-cdef-1234-567890abcdef"),
                    RoomType = RoomType.Theatre
                }
            };
        }
    }
}
