using InternerShop.Data;
using InternerShop.Models;
using InternerShop.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternerShop.Pages.Cart
{
    public class AddToCartModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CartService _cartService;

        public AddToCartModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        public bool Success { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync(int productId, int quantity = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                ErrorMessage = "Для добавления товаров в корзину необходимо войти в систему.";
                Success = false;
                return Page();
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                ErrorMessage = "Товар не найден.";
                Success = false;
                return Page();
            }

            if (product.StockQuantity < quantity)
            {
                ErrorMessage = "Недостаточно товара в наличии.";
                Success = false;
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            await _cartService.AddToCartAsync(user.Id, productId, quantity);

            ProductName = product.Name;
            Success = true;

            return Page();
        }
    }
}
