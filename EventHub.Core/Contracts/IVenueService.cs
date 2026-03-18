using EventHub.Core.Models.Venue;
using EventHub.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Core.Contracts
{
    public interface IVenueService
    {
        Task AddVenueAsync(AddVenueViewModel model, Guid userId);
        Task<IEnumerable<VenueListViewModel>> GetAllVenuesAsync();
    }
}
