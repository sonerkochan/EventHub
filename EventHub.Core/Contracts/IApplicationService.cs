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
    }
}
