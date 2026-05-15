using EventHub.Core.Contracts;
using EventHub.Core.Models.User;
using EventHub.Infrastructure.Data.Common;
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
        private readonly IRepository repo;

        public UserService(
            UserManager<User> _userManager,
            RoleManager<IdentityRole> _roleManager,
            IRepository _repo)
        {
            userManager = _userManager;
            roleManager = _roleManager;
            repo = _repo;
        }

        public async Task<IEnumerable<UserListViewModel>> GetAllUsersAsync(string? roleFilter = null)
        {
            IEnumerable<User> filteredUsers;

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                var inRole = await userManager.GetUsersInRoleAsync(roleFilter);
                filteredUsers = inRole.Where(u => !u.IsDeleted);
            }
            else
            {
                filteredUsers = await userManager.Users
                    .Where(u => !u.IsDeleted)
                    .ToListAsync();
            }

            var users = filteredUsers
                .OrderByDescending(u => u.CreatedAt)
                .ToList();

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

            await AttachRoleStatsAsync(result);

            return result;
        }

        private async Task AttachRoleStatsAsync(List<UserListViewModel> users)
        {
            if (users.Count == 0) return;

            var organizerIdStrings = users.Where(u => u.Roles.Contains("Organizer")).Select(u => u.Id).ToList();
            var supplierIdStrings = users.Where(u => u.Roles.Contains("Supplier")).Select(u => u.Id).ToList();
            var clientIdStrings = users.Where(u => u.Roles.Contains("Client")).Select(u => u.Id).ToList();

            if (organizerIdStrings.Count > 0)
            {
                var organizerGuids = organizerIdStrings
                    .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                    .Where(g => g != Guid.Empty)
                    .ToList();

                var eventStats = await repo.AllReadonly<Event>()
                    .Where(e => organizerGuids.Contains(e.OrganizerId) && e.IsActive)
                    .GroupBy(e => e.OrganizerId)
                    .Select(g => new
                    {
                        OrganizerId = g.Key,
                        EventCount = g.Count(),
                        TicketsSold = g.Sum(e => e.TicketsSold)
                    })
                    .ToListAsync();
                var eventStatsById = eventStats.ToDictionary(s => s.OrganizerId);

                var revenueByOrganizer = await repo.AllReadonly<Ticket>()
                    .Where(t => t.Status == TicketStatus.Purchased || t.Status == TicketStatus.Used)
                    .Join(repo.AllReadonly<Event>(),
                        t => t.EventId,
                        e => e.Id,
                        (t, e) => new { e.OrganizerId, t.Price })
                    .Where(x => organizerGuids.Contains(x.OrganizerId))
                    .GroupBy(x => x.OrganizerId)
                    .Select(g => new { OrganizerId = g.Key, Revenue = g.Sum(x => (decimal)x.Price) })
                    .ToListAsync();
                var revenueById = revenueByOrganizer.ToDictionary(r => r.OrganizerId, r => r.Revenue);

                foreach (var u in users.Where(u => u.Roles.Contains("Organizer")))
                {
                    if (!Guid.TryParse(u.Id, out var gid)) continue;
                    if (eventStatsById.TryGetValue(gid, out var stats))
                    {
                        u.OrganizerEventCount = stats.EventCount;
                        u.OrganizerTicketsSold = stats.TicketsSold;
                    }
                    if (revenueById.TryGetValue(gid, out var rev))
                    {
                        u.OrganizerRevenue = rev;
                    }
                }
            }

            if (supplierIdStrings.Count > 0)
            {
                var serviceCounts = await repo.AllReadonly<SupplierService>()
                    .Where(s => !s.IsDeleted && s.SupplierId != null && supplierIdStrings.Contains(s.SupplierId))
                    .GroupBy(s => s.SupplierId!)
                    .Select(g => new { SupplierId = g.Key, Count = g.Count() })
                    .ToListAsync();
                var serviceCountById = serviceCounts.ToDictionary(s => s.SupplierId, s => s.Count);

                var pendingRequests = await repo.AllReadonly<ServiceRentalRequest>()
                    .Where(r => r.Status == ServiceRentalRequestStatus.Pending)
                    .Join(repo.AllReadonly<SupplierService>(),
                        r => r.SupplierServiceId,
                        s => s.Id,
                        (r, s) => new { s.SupplierId })
                    .Where(x => x.SupplierId != null && supplierIdStrings.Contains(x.SupplierId))
                    .GroupBy(x => x.SupplierId!)
                    .Select(g => new { SupplierId = g.Key, Count = g.Count() })
                    .ToListAsync();
                var pendingById = pendingRequests.ToDictionary(p => p.SupplierId, p => p.Count);

                foreach (var u in users.Where(u => u.Roles.Contains("Supplier")))
                {
                    if (serviceCountById.TryGetValue(u.Id, out var cnt)) u.SupplierServiceCount = cnt;
                    if (pendingById.TryGetValue(u.Id, out var pending)) u.SupplierPendingRequests = pending;
                }
            }

            if (clientIdStrings.Count > 0)
            {
                var clientGuids = clientIdStrings
                    .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                    .Where(g => g != Guid.Empty)
                    .ToList();

                var ticketCounts = await repo.AllReadonly<Ticket>()
                    .Where(t => clientGuids.Contains(t.UserId)
                             && (t.Status == TicketStatus.Purchased || t.Status == TicketStatus.Used))
                    .GroupBy(t => t.UserId)
                    .Select(g => new { UserId = g.Key, Count = g.Count() })
                    .ToListAsync();
                var ticketCountById = ticketCounts.ToDictionary(t => t.UserId, t => t.Count);

                foreach (var u in users.Where(u => u.Roles.Contains("Client")))
                {
                    if (!Guid.TryParse(u.Id, out var gid)) continue;
                    if (ticketCountById.TryGetValue(gid, out var cnt)) u.ClientTicketsBought = cnt;
                }
            }
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

        public async Task<EditUserViewModel?> GetForEditAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive
            };
        }

        public async Task<(bool Success, string? Error)> CreateUserAsync(CreateUserViewModel model)
        {
            if (await userManager.FindByNameAsync(model.UserName) != null)
                return (false, "A user with that username already exists.");

            if (await userManager.FindByEmailAsync(model.Email) != null)
                return (false, "A user with that email already exists.");

            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

            if (!string.IsNullOrWhiteSpace(model.Role) && await roleManager.RoleExistsAsync(model.Role))
            {
                await userManager.AddToRoleAsync(user, model.Role);
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateUserAsync(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null) return (false, "User not found.");

            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var other = await userManager.FindByEmailAsync(model.Email);
                if (other != null && other.Id != model.Id)
                    return (false, "Another user already uses that email.");

                user.Email = model.Email;
                user.NormalizedEmail = userManager.NormalizeEmail(model.Email);
                user.EmailConfirmed = false;
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

            return (true, null);
        }
    }
}
