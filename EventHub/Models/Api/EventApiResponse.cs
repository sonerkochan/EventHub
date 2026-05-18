namespace EventHub.Models.Api
{
    public class EventApiResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Description { get; set; }
        public decimal TicketPrice { get; set; }
        public string Currency { get; set; } = "EUR";
        public int AvailableTickets { get; set; }
        public int TotalTickets { get; set; }
        public int TicketsSold { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? RoomName { get; set; }
        public EventApiLocationResponse Location { get; set; } = new();
    }

    public class EventApiLocationResponse
    {
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public string? City { get; set; }
        public string? CountryCode { get; set; }
        public string? Address { get; set; }
    }
}
