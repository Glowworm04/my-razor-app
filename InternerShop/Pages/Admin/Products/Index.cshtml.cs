using InternerShop.Data;
using InternerShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = new List<Product>();
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Status { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalProducts { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();

            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // Фильтрация
            if (!string.IsNullOrEmpty(SearchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(SearchString));
            }

            if (CategoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == CategoryId.Value);
            }

            if (!string.IsNullOrEmpty(Status))
            {
                productsQuery = Status switch
                {
                    "active" => productsQuery.Where(p => p.IsActive),
                    "inactive" => productsQuery.Where(p => !p.IsActive),
                    "lowstock" => productsQuery.Where(p => p.StockQuantity < 10),
                    _ => productsQuery
                };
            }

            // Пагинация
            TotalProducts = await productsQuery.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalProducts / (double)PageSize);

            Products = await productsQuery
                .OrderByDescending(p => p.CreatedDate)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Товар успешно удален";
            return RedirectToPage();
        }
    }
}
