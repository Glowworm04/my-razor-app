using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Models
{
    public class ProductImage
    {
        [Key]
        public int ProductImageId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(200)]
        public string? AltText { get; set; }

        public bool IsPrimary { get; set; } = false;

        // Внешний ключ для товара
        public int ProductId { get; set; }

        // Навигационные свойства
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
