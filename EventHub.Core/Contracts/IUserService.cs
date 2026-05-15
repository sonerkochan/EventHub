using EventHub.Core.Models.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHub.Core.Contracts
{
    public interface IUserService
    {
        Task<IEnumerable<UserListViewModel>> GetAllUsersAsync(string? roleFilter = null);
        Task<UserDetailViewModel?> GetUserByIdAsync(string userId);
        Task<EditUserViewModel?> GetForEditAsync(string userId);
        Task<(bool Success, string? Error)> CreateUserAsync(CreateUserViewModel model);
        Task<(bool Success, string? Error)> UpdateUserAsync(EditUserViewModel model);
        Task<bool> DeactivateUserAsync(string userId);
        Task<bool> ReactivateUserAsync(string userId);
        Task<bool> DeleteUserAsync(string userId);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
        Task<bool> AddRoleToUserAsync(string userId, string role);
        Task<bool> RemoveRoleFromUserAsync(string userId, string role);
    }
}