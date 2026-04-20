using EventHub.Core.Models.Venue;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHub.Core.Contracts
{
    public interface IVenueService
    {
        Task AddVenueAsync(AddVenueViewModel model, Guid userId);
        Task<IEnumerable<VenueListViewModel>> GetAllVenuesAsync();
        Task<VenueDetailViewModel?> GetByIdAsync(Guid id);
        Task<EditVenueViewModel?> GetForEditAsync(Guid id);
        Task<bool> UpdateAsync(EditVenueViewModel model);
        Task<bool> DeactivateAsync(Guid id);
    }
}
