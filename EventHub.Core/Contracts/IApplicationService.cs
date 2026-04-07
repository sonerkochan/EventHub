using EventHub.Core.Models.ApplicationForm;
using EventHub.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHub.Core.Contracts
{
    public interface IApplicationService
    {
        Task<bool> ApplyAsync(string userId, ApplicationFormViewModel model);
        Task<IEnumerable<ApplicationListViewModel>> GetAllPendingAsync();
        Task<bool> ApproveAsync(int applicationId, string adminUserName);
        Task<bool> RejectAsync(int applicationId, string adminUserName, string comment);
    }
}
