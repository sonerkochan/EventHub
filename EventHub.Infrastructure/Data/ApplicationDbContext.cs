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
            builder.ApplyConfiguration(new RoomConfiguration());

            base.OnModelCreating(builder);

            builder.Entity<Venue>(v => v.HasKey(v => v.Id));
            builder.Entity<Room>(r => r.HasKey(r => r.RoomId));
            builder.Entity<Seat>(s => s.HasKey(s => s.Id));
            builder.Entity<Zone>(z => z.HasKey(z => z.Id));
            builder.Entity<SeatLayout>(sl => sl.HasKey(sl => sl.Id));
            builder.Entity<EmailVerificationToken>(ev => ev.HasKey(ev => ev.TokenId));
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<SeatLayout> SeatLayouts { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    }
}