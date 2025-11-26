using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Models
{
    public class Region
    {
        [Key]
        public int RegionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Внешний ключ для страны
        public int CountryId { get; set; }

        // Навигационные свойства
        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; } = null!;

        public virtual ICollection<City> Cities { get; set; } = new List<City>();
    }
}
