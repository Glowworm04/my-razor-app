using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Models
{
    public class UserAddress
    {
        [Key]
        public int UserAddressId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [StringLength(50)]
        public string AddressType { get; set; } = "Home";

        [StringLength(20)]
        public string? HouseNumber { get; set; }

        [StringLength(20)]
        public string? ApartmentNumber { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        public bool IsPrimary { get; set; } = false;

        // Внешние ключи для адресных компонентов
        public int StreetId { get; set; }

        // Навигационные свойства
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("StreetId")]
        public virtual Street Street { get; set; } = null!;
    }
}
