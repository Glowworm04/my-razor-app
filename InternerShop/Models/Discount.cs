using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Models
{
    public class Discount
    {
        [Key]
        public int DiscountId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Добавляем поле для изображения
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual ICollection<ProductDiscount> ProductDiscounts { get; set; } = new List<ProductDiscount>();
    }
}
