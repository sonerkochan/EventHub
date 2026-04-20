using EventHub.Core.Contracts;
using EventHub.Core.Models.EventPricingTier;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DataPricingTier = EventHub.Infrastructure.Data.Models.EventPricingTier;

namespace EventHub.Core.Services
{
    public class EventPricingTierService : IEventPricingTierService
    {
        private readonly IRepository repo;

        public EventPricingTierService(IRepository _repo)
        {
            repo = _repo;
        }

        public async Task<Guid> CreateAsync(CreatePricingTierViewModel model)
        {
            var entity = new DataPricingTier
            {
                Id = Guid.NewGuid(),
                EventId = model.EventId,
                ZoneId = model.ZoneId,
                TierName = model.TierName,
                Price = model.Price,
                Currency = model.Currency,
                AvailableQuantity = model.AvailableQuantity,
                SoldQuantity = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<IEnumerable<PricingTierListViewModel>> GetByEventAsync(Guid eventId)
        {
            return await repo.AllReadonly<DataPricingTier>()
                .Where(pt => pt.EventId == eventId && pt.IsActive)
                .Join(
                    repo.AllReadonly<Event>(),
                    pt => pt.EventId,
                    e => e.Id,
                    (pt, e) => new { pt, e })
                .GroupJoin(
                    repo.AllReadonly<Zone>(),
                    x => x.pt.ZoneId,
                    z => z.Id,
                    (x, zones) => new { x.pt, x.e, zones })
                .SelectMany(
                    x => x.zones.DefaultIfEmpty(),
                    (x, z) => new PricingTierListViewModel
                    {
                        Id = x.pt.Id,
                        EventId = x.pt.EventId,
                        EventName = x.e.EventName,
                        ZoneId = x.pt.ZoneId,
                        ZoneName = z != null ? z.Name : null,
                        TierName = x.pt.TierName,
                        Price = x.pt.Price,
                        Currency = x.pt.Currency,
                        AvailableQuantity = x.pt.AvailableQuantity,
                        SoldQuantity = x.pt.SoldQuantity,
                        IsActive = x.pt.IsActive
                    })
                .OrderBy(pt => pt.Price)
                .ToListAsync();
        }

        public async Task<EditPricingTierViewModel?> GetForEditAsync(Guid id)
        {
            var entity = await repo.GetByIdAsync<DataPricingTier>(id);
            if (entity == null) return null;

            return new EditPricingTierViewModel
            {
                Id = entity.Id,
                EventId = entity.EventId,
                ZoneId = entity.ZoneId,
                TierName = entity.TierName!,
                Price = entity.Price,
                Currency = entity.Currency ?? "USD",
                AvailableQuantity = entity.AvailableQuantity
            };
        }

        public async Task<bool> UpdateAsync(EditPricingTierViewModel model)
        {
            var entity = await repo.GetByIdAsync<DataPricingTier>(model.Id);
            if (entity == null) return false;

            entity.ZoneId = model.ZoneId;
            entity.TierName = model.TierName;
            entity.Price = model.Price;
            entity.Currency = model.Currency;
            entity.AvailableQuantity = model.AvailableQuantity;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var entity = await repo.GetByIdAsync<DataPricingTier>(id);
            if (entity == null) return false;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateAsync(Guid id)
        {
            var entity = await repo.GetByIdAsync<DataPricingTier>(id);
            if (entity == null) return false;

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }
    }
}
