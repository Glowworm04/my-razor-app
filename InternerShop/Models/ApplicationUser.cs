using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
    }
}
