using EventHub.Core.Contracts;
using EventHub.Core.Models.User;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventHub.Core.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserService(UserManager<User> _userManager, RoleManager<IdentityRole> _roleManager)
        {
            userManager = _userManager;
            roleManager = _roleManager;
            Console.WriteLine("Hell Yuhhhh Boy");
        }

        public async Task<IEnumerable<UserListViewModel>> GetAllUsersAsync()
        {
            var users = await userManager.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var result = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                result.Add(new UserListViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    Roles = roles.ToList()
                });
            }

            return result;
        }

        public async Task<UserDetailViewModel?> GetUserByIdAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await userManager.GetRolesAsync(user);

            return new UserDetailViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                DeletedAt = user.DeletedAt,
                Roles = roles.ToList()
            };
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ReactivateUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Enumerable.Empty<string>();

            return await userManager.GetRolesAsync(user);
        }

        public async Task<bool> AddRoleToUserAsync(string userId, string role)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!await roleManager.RoleExistsAsync(role)) return false;

            var result = await userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string role)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await userManager.RemoveFromRoleAsync(user, role);
            return result.Succeeded;
        }
    }
}
