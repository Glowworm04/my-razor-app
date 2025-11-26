using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace InternerShop.Models
{
    public class City
    {
        [Key]
        public int CityId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Внешний ключ для региона
        public int RegionId { get; set; }

        // Навигационные свойства
        [ForeignKey("RegionId")]
        public virtual Region Region { get; set; } = null!;

        public virtual ICollection<Street> Streets { get; set; } = new List<Street>();
    }
}
