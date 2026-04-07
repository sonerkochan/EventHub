using EventHub.Core.Contracts;
using EventHub.Core.Models.ApplicationForm;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IRepository repository;
        private readonly UserManager<User> userManager;

        public ApplicationService(IRepository _repository, UserManager<User> _userManager)
        {
            repository = _repository;
            userManager = _userManager;
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

        public async Task<IEnumerable<ApplicationListViewModel>> GetAllPendingAsync()
        {
            return await repository.All<ApplicationForm>()
                .Where(a => a.Status == ApplicationStatus.Pending)
                .Select(a => new ApplicationListViewModel
                {
                    Id = a.Id,
                    UserName = a.User.UserName!,
                    Type = a.Type,
                    Description = a.Description ?? "",
                    OrganizationName = a.OrganizationName,
                    PhoneNumber = a.PhoneNumber,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> ApproveAsync(int applicationId, string adminUserId)
        {
            var app = await repository.All<ApplicationForm>()
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app == null || app.Status != ApplicationStatus.Pending)
                return false;

            app.Status = ApplicationStatus.Approved;
            app.ReviewedById = adminUserId;
            app.ReviewedAt = DateTime.UtcNow;

            if (!await userManager.IsInRoleAsync(app.User, app.Type.ToString()))
            {
                await userManager.AddToRoleAsync(app.User, app.Type.ToString());
            }

            await repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int applicationId, string adminUserId, string comment)
        {
            var app = await repository.All<ApplicationForm>()
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app == null || app.Status != ApplicationStatus.Pending)
                return false;

            app.Status = ApplicationStatus.Rejected;
            app.ReviewedById = adminUserId;
            app.ReviewComment = comment;
            app.ReviewedAt = DateTime.UtcNow;

            await repository.SaveChangesAsync();
            return true;
        }
    }
}