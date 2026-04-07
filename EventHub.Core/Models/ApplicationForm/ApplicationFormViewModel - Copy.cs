using EventHub.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Core.Models.ApplicationForm
{
    public class ApplicationListViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public ApplicationType Type { get; set; }
        public string Description { get; set; } = null!;
        public string? OrganizationName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
