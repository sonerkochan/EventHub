using EventHub.Core.Contracts;
using EventHub.Core.Models.Payment;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventHubServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IVenueService, VenueService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IModeratorService, ModeratorService>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IPaymentService, StripePaymentService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ISeatLayoutService, SeatLayoutService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IRefundService, RefundService>();
            services.AddScoped<ISeatService, SeatService>();
            services.AddScoped<IZoneService, ZoneService>();
            services.AddScoped<IEventPricingTierService, EventPricingTierService>();
            services.AddScoped<IQRCodeService, QRCodeService>();
            services.AddScoped<ISupplierServiceCatalogService, SupplierServiceCatalogService>();

            return services;
        }

        public static IServiceCollection AddStripe(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.Section));
            return services;
        }
    }
}
