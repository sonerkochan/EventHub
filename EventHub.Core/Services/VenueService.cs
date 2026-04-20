using EventHub.Core.Contracts;
using EventHub.Core.Models.Venue;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EventHub.Core.Services
{
    public class VenueService : IVenueService
    {
        private readonly IRepository repo;

        public VenueService(IRepository _repo)
        {
            repo = _repo;
        }

        [Description("Creates a new venue and adds it to the database.")]
        public async Task AddVenueAsync(AddVenueViewModel model, Guid userId)
        {
            var entity = new Venue()
            {
                Id = Guid.NewGuid(),
                CreatedBy = userId,

                Name = model.Name,
                Description = model.Description,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                PostalCode = model.PostalCode,

                Latitude = model.Latitude,
                Longitude = model.Longitude,

                ContactEmail = model.ContactEmail,
                ContactPhone = model.ContactPhone,

                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
        }

        [Description("Returns all existing Venues from the database.")]
        public async Task<IEnumerable<VenueListViewModel>> GetAllVenuesAsync()
        {
            return await repo.AllReadonly<Venue>()
                .Where(v => v.IsActive)
                .Select(v => new VenueListViewModel
                {
                    Id = v.Id,
                    Name = v.Name,
                    City = v.City,
                    Country = v.Country,
                    Address = v.Address,
                    Latitude = v.Latitude,
                    Longitude = v.Longitude
                })
                .ToListAsync();
        }

        public async Task<VenueDetailViewModel?> GetByIdAsync(Guid id)
        {
            return await repo.AllReadonly<Venue>()
                .Where(v => v.Id == id && v.IsActive)
                .Select(v => new VenueDetailViewModel
                {
                    Id = v.Id,
                    Name = v.Name!,
                    Description = v.Description,
                    Address = v.Address,
                    City = v.City,
                    Country = v.Country,
                    PostalCode = v.PostalCode,
                    Latitude = v.Latitude,
                    Longitude = v.Longitude,
                    ContactEmail = v.ContactEmail,
                    ContactPhone = v.ContactPhone,
                    IsActive = v.IsActive,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<EditVenueViewModel?> GetForEditAsync(Guid id)
        {
            var entity = await repo.AllReadonly<Venue>()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (entity == null) return null;

            return new EditVenueViewModel
            {
                Id = entity.Id,
                Name = entity.Name!,
                Description = entity.Description,
                Address = entity.Address!,
                City = entity.City!,
                Country = entity.Country!,
                PostalCode = entity.PostalCode,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                ContactEmail = entity.ContactEmail,
                ContactPhone = entity.ContactPhone
            };
        }

        public async Task<bool> UpdateAsync(EditVenueViewModel model)
        {
            var entity = await repo.All<Venue>()
                .FirstOrDefaultAsync(v => v.Id == model.Id);

            if (entity == null) return false;

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Address = model.Address;
            entity.City = model.City;
            entity.Country = model.Country;
            entity.PostalCode = model.PostalCode;
            entity.Latitude = model.Latitude;
            entity.Longitude = model.Longitude;
            entity.ContactEmail = model.ContactEmail;
            entity.ContactPhone = model.ContactPhone;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var entity = await repo.All<Venue>()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (entity == null) return false;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await repo.SaveChangesAsync();
            return true;
        }
    }
}