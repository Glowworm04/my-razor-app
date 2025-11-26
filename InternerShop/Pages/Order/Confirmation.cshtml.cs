using InternerShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Pages.Order
{
    public class ConfirmationModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ConfirmationModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Email { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            OrderId = order.OrderId;
            TotalAmount = order.TotalAmount;
            OrderStatus = order.OrderStatus;
            OrderDate = order.OrderDate;
            Email = order.User.Email;

            return Page();
        }
    }
}
