using EventHub.Infrastructure.Data.Configuration;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new RoleConfiguration());
            builder.ApplyConfiguration(new UserConfiguration());
            builder.ApplyConfiguration(new UserRoleConfiguration());
            base.OnModelCreating(builder);

            builder.Entity<Venue>(v => v.HasKey(v => v.Id));
            builder.Entity<Room>(r => r.HasKey(r => r.RoomId));
            builder.Entity<EmailVerificationToken>(ev => ev.HasKey(ev => ev.TokenId));
        }

        DbSet<Venue> Venues { get; set; }
        DbSet<Room> Rooms { get; set; }
        DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    }
}