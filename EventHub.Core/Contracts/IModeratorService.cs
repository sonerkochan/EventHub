using EventHub.Core.Models.Moderator;
using EventHub.Core.Models.Venue;
using EventHub.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Core.Contracts
{
    public interface IModeratorService
    {
        Task<IEnumerable<ModeratorListViewModel>> GetAllModeratorsAsync();
        Task<bool> CreateModeratorAsync(AddModeratorViewModel model);
        Task<EditModeratorViewModel?> GetModeratorByIdAsync(string id);
        Task<bool> EditModeratorAsync(EditModeratorViewModel model);
        Task<bool> SetActiveStatusAsync(string id, bool isActive);
    }
}
