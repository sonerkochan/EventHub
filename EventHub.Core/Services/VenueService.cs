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
    }
}