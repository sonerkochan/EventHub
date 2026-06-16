using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Data.Configuration
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>  // ← User not IdentityUser
    {
        public void Configure(EntityTypeBuilder<User> builder)  // ← User not IdentityUser
        {
            builder.HasData(CreateDefaultAdmin());
        }

        private User CreateDefaultAdmin()
        {
            var admin = new User()
            {
                Id = "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@eventhub.com",
                NormalizedEmail = "ADMIN@EVENTHUB.COM",
                EmailConfirmed = true,
                SecurityStamp = "STATIC_SECURITY_STAMP_ADMIN_001",
                ConcurrencyStamp = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                IsActive = true,
                CreatedAt = new DateTime(2026, 5, 29, 10, 55, 8, 27, DateTimeKind.Utc).AddTicks(1987),
                UpdatedAt = new DateTime(2026, 5, 29, 10, 55, 8, 27, DateTimeKind.Utc).AddTicks(1993),
                PasswordHash = "AQAAAAIAAYagAAAAEG38AuyGuEieGSgIjw2GqBwsTbF345G+3m8+MGVgIE5WOd/6dwXLH196K/iTJNEZxA=="
            };

            return admin;
        }
    }
}
