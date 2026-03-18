using EventHub.Core.Contracts;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventHubServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IVenueService, VenueService>();


            return services;
        }
    }
}
