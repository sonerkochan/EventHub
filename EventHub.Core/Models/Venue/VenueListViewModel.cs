using System;

namespace EventHub.Core.Models.Venue
{
    public class VenueListViewModel
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        public string? Address { get; set; }

        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}