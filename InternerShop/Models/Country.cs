using System.ComponentModel.DataAnnotations;

namespace InternerShop.Models
{
    public class Country
    {
        [Key]
        public int CountryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(10)]
        public string? CountryCode { get; set; }

        // Навигационные свойства
        public virtual ICollection<Region> Regions { get; set; } = new List<Region>();
    }
}
