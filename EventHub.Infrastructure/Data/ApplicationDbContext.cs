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
            builder.Entity<PaymentTicket>().HasKey(pt => new { pt.PaymentId, pt.TicketId });
            builder.Entity<SupplierService>(s =>
            {
                s.HasKey(x => x.Id);
                s.HasQueryFilter(x => !x.IsDeleted);
            });
            builder.Entity<ServiceRentalRequest>(r =>
            {
                r.HasKey(x => x.Id);
                r.HasOne(x => x.SupplierService)
                    .WithMany()
                    .HasForeignKey(x => x.SupplierServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
                r.HasOne(x => x.Requester)
                    .WithMany()
                    .HasForeignKey(x => x.RequesterId)
                    .OnDelete(DeleteBehavior.Restrict);
                r.HasOne(x => x.ReviewedBy)
                    .WithMany()
                    .HasForeignKey(x => x.ReviewedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Event>().Property(e => e.BasePrice).HasPrecision(18, 4);
            builder.Entity<Event>().Property(e => e.Latitude).HasPrecision(10, 7);
            builder.Entity<Event>().Property(e => e.Longitude).HasPrecision(10, 7);
            builder.Entity<Event>()
                .HasOne(e => e.CoverImage)
                .WithMany()
                .HasForeignKey(e => e.CoverPhotoId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<SeatLayout> SeatLayouts { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<EventPricingTier> EventPricingTiers { get; set; }
        public DbSet<EventTrustScore> EventTrustScores { get; set; }
        public DbSet<OrganizerTrustScore> OrganizerTrustScores { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentTicket> PaymentTickets { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewVote> ReviewVotes { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<ApplicationForm> ApplicationForms { get; set; }
        public DbSet<SeatHolds> SeatHolds { get; set; }
        public DbSet<SupplierService> SupplierServices { get; set; }
        public DbSet<ServiceRentalRequest> ServiceRentalRequests { get; set; }
        public DbSet<Photo> CoverPhotos { get; set; }

    }
}
