using EventHub.Infrastructure.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Core.Models.Room
{
    public class EditRoomViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid VenueId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public long Capacity { get; set; }

        [Required]
        public RoomType RoomType { get; set; }
    }
}
