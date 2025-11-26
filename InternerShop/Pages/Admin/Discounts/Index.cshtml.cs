using InternerShop.Data;
using InternerShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Pages.Admin.Discounts
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Discount> Discounts { get; set; } = new List<Discount>();

        public async Task OnGetAsync()
        {
            Discounts = await _context.Discounts
                .OrderByDescending(d => d.StartDate) // Сортируем по дате начала вместо CreatedDate
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Скидка успешно удалена";
            return RedirectToPage();
        }
    }
}
