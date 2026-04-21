using System.ComponentModel.DataAnnotations;

namespace EventHub.Areas.Supplier.Models
{
    public class ServiceCreateViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Service Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal? Price { get; set; }
    }
}
