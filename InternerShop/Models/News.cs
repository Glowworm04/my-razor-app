using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Models
{
    public class News
    {
        [Key]
        public int NewsId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required]
        public bool IsPublished { get; set; } = false;

        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string AuthorId { get; set; } = string.Empty;

        // Навигационные свойства
        [ForeignKey("AuthorId")]
        public virtual ApplicationUser Author { get; set; } = null!;
    }
}
