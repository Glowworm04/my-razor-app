using System.ComponentModel.DataAnnotations;

namespace InternerShop.Models
{
    public class ShoppingCart
    {
        [Key]
        public int CartId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
