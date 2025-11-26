using InternerShop.Data;
using InternerShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Pages.Catalog
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Product Product { get; set; } = new Product();
        public List<Review> Reviews { get; set; } = new List<Review>();

        [BindProperty]
        public ReviewInputModel ReviewInput { get; set; } = new ReviewInputModel();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Product = await _context.Products
        .Include(p => p.Category)
        .Include(p => p.ProductImages)
        .Include(p => p.Reviews)
            .ThenInclude(r => r.User)
        .FirstOrDefaultAsync(p => p.ProductId == id);

            if (Product == null)
            {
                return NotFound();
            }

            Reviews = await _context.Reviews
                .Where(r => r.ProductId == id && r.IsApproved)
                .Include(r => r.User)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddReviewAsync(int productId)
        {
            if (!ModelState.IsValid)
            {
                return await OnGetAsync(productId);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            var review = new Review
            {
                UserId = user.Id,
                ProductId = productId,
                Rating = ReviewInput.Rating,
                Comment = ReviewInput.Comment,
                ReviewDate = DateTime.UtcNow,
                IsApproved = false // Требует одобрения администратора
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = productId });
        }
    }

    public class ReviewInputModel
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }
}
