using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Data.Configuration
{
    internal class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            builder.HasData(new IdentityUserRole<string>()
            {
                // Admin User
                UserId = "f7a1b2c3-d4e5-6789-abcd-ef0123456789",
                // Admin Role
                RoleId = "d9de7285-b674-454c-9889-5210abb8d347"
            });
        }
    }
}