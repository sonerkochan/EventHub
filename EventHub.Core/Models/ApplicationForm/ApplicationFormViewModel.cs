using EventHub.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Core.Models.ApplicationForm
{
    public class ApplicationFormViewModel
    {

        [Required]
        public ApplicationType Type { get; set; }

        [Required]
        public string Name { get; set; } = null!; // Organization or Business

        [Required]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;
    }
}
