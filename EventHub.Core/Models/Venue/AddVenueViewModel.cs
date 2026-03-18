using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Core.Models.Venue
{
    public class AddVenueViewModel
    {
        [Required]
        public Guid CreatedBy { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string Address { get; set; } = null!;

        [Required]
        public string City { get; set; } = null!;

        [Required]
        public string Country { get; set; } = null!;

        public string? PostalCode { get; set; }

        public float Latitude { get; set; }
        public float Longitude { get; set; }

        [EmailAddress]
        public string? ContactEmail { get; set; }

        public string? ContactPhone { get; set; }
    }
}
