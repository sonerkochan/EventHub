using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHub.Infrastructure.Data.Models
{
    public class Venue
    {
        //[Key]
        public Guid Id { get; set; }

        //Id of user created by
        public Guid CreatedBy { get; set; }
        public string? Name { get; set; } = null;
        public string? Description { get; set; } = null;
        public string? Address { get; set; } = null;
        public string? City { get; set; } = null;
        public string? Country { get; set; } = null;
        public string? PostalCode { get; set; } = null;
        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public string? ContactEmail { get; set; } = null;
        public string? ContactPhone { get; set; } = null;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}
