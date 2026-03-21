using EventHub.Core.Contracts;
using EventHub.Core.Models.Moderator;
using EventHub.Core.Models.Venue;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EventHub.Core.Services
{
    public class ModeratorService : IModeratorService
    {
        private readonly UserManager<User> userManager;

        public ModeratorService(UserManager<User> _userManager)
        {
            userManager = _userManager;
        }

        public async Task<IEnumerable<ModeratorListViewModel>> GetAllModeratorsAsync()
        {
            var moderators = await userManager.GetUsersInRoleAsync("Moderator");

            return moderators.Select(u => new ModeratorListViewModel
            {
                Id = u.Id,
                Username = u.UserName!,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            });
        }

        public async Task<bool> CreateModeratorAsync(AddModeratorViewModel model)
        {
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return false;

            await userManager.AddToRoleAsync(user, "Moderator");
            return true;
        }

        public async Task<EditModeratorViewModel?> GetModeratorByIdAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
                return null;

            return new EditModeratorViewModel
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty
            };
        }

        public async Task<bool> EditModeratorAsync(EditModeratorViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);

            if (user == null)
                return false;

            user.UserName = model.Username;
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> SetActiveStatusAsync(string id, bool isActive)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
                return false;

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}