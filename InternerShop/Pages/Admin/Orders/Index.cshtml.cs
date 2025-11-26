using InternerShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Pages.Admin.Orders
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Models.Order> Orders { get; set; } = new List<Models.Order>();

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateTo { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalOrders { get; set; }

        public async Task OnGetAsync()
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            // Фильтрация по статусу
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                ordersQuery = ordersQuery.Where(o => o.OrderStatus == StatusFilter);
            }

            // Фильтрация по дате
            if (DateFrom.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate >= DateFrom.Value);
            }

            if (DateTo.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate <= DateTo.Value.AddDays(1));
            }

            // Пагинация
            TotalOrders = await ordersQuery.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalOrders / (double)PageSize);

            Orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Pending" => "warning",
                "Processing" => "info",
                "Shipped" => "primary",
                "Delivered" => "success",
                "Cancelled" => "danger",
                _ => "secondary"
            };
        }
    }
}
