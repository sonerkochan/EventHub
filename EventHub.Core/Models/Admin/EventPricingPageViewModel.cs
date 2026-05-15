using System;
using System.Collections.Generic;

namespace EventHub.Core.Models.Admin
{
    public class EventPricingPageViewModel
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public Guid RoomId { get; set; }
        public string? RoomName { get; set; }
        public string DefaultCurrency { get; set; } = "EUR";
        public List<ZonePricingRow> Zones { get; set; } = new();
        public bool AllZonesPriced => Zones.Count > 0 && Zones.TrueForAll(z => z.IsConfigured);
    }
}
