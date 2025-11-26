using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Models
{
    public class ProductDiscount
    {
        [Key]
        public int ProductDiscountId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int DiscountId { get; set; }

        // Навигационные свойства
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("DiscountId")]
        public virtual Discount Discount { get; set; } = null!;
    }
}
