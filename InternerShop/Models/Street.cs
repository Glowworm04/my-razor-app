using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Models
{
    public class Street
    {
        [Key]
        public int StreetId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        // Внешний ключ для города
        public int CityId { get; set; }

        // Навигационные свойства
        [ForeignKey("CityId")]
        public virtual City City { get; set; } = null!;

        public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
    }
}
