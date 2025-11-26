using InternerShop.Data;
using InternerShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public List<News> LatestNews { get; set; } = new List<News>();
        public List<Product> PopularProducts { get; set; } = new List<Product>();
        public List<Discount> ActiveDiscounts { get; set; } = new List<Discount>();

        public async Task OnGetAsync()
        {
            LatestNews = await _context.News
                .Where(n => n.IsPublished && n.PublishedDate <= DateTime.UtcNow)
                .OrderByDescending(n => n.PublishedDate)
                .Take(3)
                .Include(n => n.Author)
                .ToListAsync();

            PopularProducts = await _context.Products
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedDate)
                .Take(4)
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .ToListAsync();

            ActiveDiscounts = await _context.Discounts
                .Where(d => d.IsActive && d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
                .Take(2)
                .ToListAsync();
        }
    }
}
