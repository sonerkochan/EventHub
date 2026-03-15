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
            NormalizedName = "ADMIN"
        },
        new IdentityRole()
        {
            Id = "07358494-247c-421c-8f7f-82c12be55276",
            Name = "Client",
            NormalizedName = "CLIENT"
        },
        new IdentityRole()
        {
            Id = "b2c3d4e5-f6a7-8901-bcde-f01234567891",
            Name = "Supplier",
            NormalizedName = "SUPPLIER"
        },
        new IdentityRole()
        {
            Id = "c3d4e5f6-a7b8-9012-cdef-012345678912",
            Name = "Organizer",
            NormalizedName = "ORGANIZER"
        },
    };
        }
    }
}