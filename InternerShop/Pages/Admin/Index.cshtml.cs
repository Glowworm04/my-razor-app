using InternerShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using InternerShop.Models;
using Microsoft.AspNetCore.Authorization;

namespace InternerShop.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockProducts { get; set; }
        public int ActiveDiscounts { get; set; }
        public int PublishedNews { get; set; }
        public List<Models.Order> RecentOrders { get; set; } = new List<Models.Order>();

        public async Task OnGetAsync()
        {
            TotalOrders = await _context.Orders.CountAsync();
            TotalProducts = await _context.Products.CountAsync();
            TotalUsers = await _context.Users.CountAsync();
            PendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Pending");
            LowStockProducts = await _context.Products.CountAsync(p => p.StockQuantity < 10);
            ActiveDiscounts = await _context.Discounts.CountAsync(d => d.IsActive && d.EndDate >= DateTime.UtcNow);
            PublishedNews = await _context.News.CountAsync(n => n.IsPublished);

            RecentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();
        }
    }
}
