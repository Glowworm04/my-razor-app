using InternerShop.Data;
using InternerShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Pages.Admin.Orders
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Order Order { get; set; } = new Models.Order();
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        [BindProperty]
        public int OrderId { get; set; }

        [BindProperty]
        public string OrderStatus { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (Order == null)
            {
                return NotFound();
            }

            OrderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == id)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.ProductImages)
                .ToListAsync();

            OrderId = id;
            OrderStatus = Order.OrderStatus;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var order = await _context.Orders.FindAsync(OrderId);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = OrderStatus;

            // Обновляем даты в зависимости от статуса
            if (OrderStatus == "Shipped" && order.ShippedDate == null)
            {
                order.ShippedDate = DateTime.UtcNow;
            }
            else if (OrderStatus == "Delivered" && order.DeliveredDate == null)
            {
                order.DeliveredDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Статус заказа успешно обновлен";
            return RedirectToPage("./Details", new { id = OrderId });
        }
    }
}
