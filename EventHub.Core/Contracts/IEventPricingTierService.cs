using EventHub.Core.Models.Admin;
using EventHub.Core.Models.EventPricingTier;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHub.Core.Contracts
{
    public interface IEventPricingTierService
    {
        Task<Guid> CreateAsync(CreatePricingTierViewModel model);
        Task<IEnumerable<PricingTierListViewModel>> GetByEventAsync(Guid eventId);
        Task<EditPricingTierViewModel?> GetForEditAsync(Guid id);
        Task<bool> UpdateAsync(EditPricingTierViewModel model);
        Task<bool> DeactivateAsync(Guid id);
        Task<bool> ActivateAsync(Guid id);
        Task<Guid> SetForZoneAsync(SetZonePriceRequest request);
        Task<bool> RemoveForZoneAsync(Guid eventId, Guid zoneId);
    }
}
