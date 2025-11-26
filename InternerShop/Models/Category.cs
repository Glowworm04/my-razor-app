using System.ComponentModel.DataAnnotations;

namespace InternerShop.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Навигационные свойства
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
