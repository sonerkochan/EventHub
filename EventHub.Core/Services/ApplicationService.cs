using EventHub.Core.Contracts;
using EventHub.Core.Models.ApplicationForm;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IRepository repository;

        public ApplicationService(IRepository _repository)
        {
            repository = _repository;
        }

        public async Task<bool> ApplyAsync(string userId, ApplicationFormViewModel model)
        {
            var exists = await repository.All<ApplicationForm>()
                .AnyAsync(a => a.UserId == userId
                            && a.Type == model.Type
                            && a.Status == ApplicationStatus.Pending);

            if (exists)
                return false;

            var application = new ApplicationForm
            {
                UserId = userId,
                Type = model.Type,
                Status = ApplicationStatus.Pending,
                Description = model.Description,
                OrganizationName = model.Name,
                PhoneNumber = model.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            await repository.AddAsync(application);
            await repository.SaveChangesAsync();

            return true;
        }
    }
}
