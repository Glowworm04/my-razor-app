using InternerShop.Data;
using InternerShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Pages.Catalog
{
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
        public string SortOrder { get; set; } = "name";

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 8;
        public int TotalPages { get; set; }
        public int TotalProducts { get; set; }

        public async Task OnGetAsync()
        {
            // Получение категорий для фильтра
            Categories = await _context.Categories
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();

            // Базовый запрос
            var productsQuery = _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // Применение фильтров
            if (!string.IsNullOrEmpty(SearchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(SearchString) ||
                    p.Description.Contains(SearchString));
            }

            if (CategoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == CategoryId.Value);
            }

            // Применение сортировки
            productsQuery = SortOrder switch
            {
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                "newest" => productsQuery.OrderByDescending(p => p.CreatedDate),
                _ => productsQuery.OrderBy(p => p.Name)
            };

            // Пагинация
            TotalProducts = await productsQuery.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalProducts / (double)PageSize);

            Products = await productsQuery
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
