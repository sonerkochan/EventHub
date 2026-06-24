    using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Data.Configuration
{
    internal class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(CreateRoles());
        }

        private List<IdentityRole> CreateRoles()
        {
            return new List<IdentityRole>()
    {
        new IdentityRole()
        {
            Id = "d9de7285-b674-454c-9889-5210abb8d347",
            Name = "Admin",
            NormalizedName = "ADMIN",
            ConcurrencyStamp = "681d155b-51a5-480e-9f27-42dcb3ab7015"
        },
        new IdentityRole()
        {
            Id = "07358494-247c-421c-8f7f-82c12be55276",
            Name = "Client",
            NormalizedName = "CLIENT",
            ConcurrencyStamp = "7deac7e2-8225-40c3-8a6e-c6480ceff30a"
        },
        new IdentityRole()
        {
            Id = "b2c3d4e5-f6a7-8901-bcde-f01234567891",
            Name = "Supplier",
            NormalizedName = "SUPPLIER",
            ConcurrencyStamp = "1caded3c-b521-4de5-8842-df966af45be3"
        },
        new IdentityRole()
        {
            Id = "c3d4e5f6-a7b8-9012-cdef-012345678912",
            Name = "Organizer",
            NormalizedName = "ORGANIZER",
            ConcurrencyStamp = "8566a253-7a5d-434a-93f7-f9e30e4c851b"
        },
        new IdentityRole()
        {
            Id = "e4f5a6b7-c8d9-0123-def0-123456789abc",
            Name = "Moderator",
            NormalizedName = "MODERATOR",
            ConcurrencyStamp = "0ecf719b-1baf-4b95-9ae6-109b2c3058bf"
        },
    };
        }
    }
}
