using InternerShop.Data;
using InternerShop.Models;
using InternerShop.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CartService _cartService;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }

        public async Task OnGetAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var cart = await _cartService.GetOrCreateCartAsync(user.Id);
                CartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.Category)
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                    .ToListAsync();

                TotalAmount = await _cartService.GetCartTotalAsync(user.Id);
            }
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int cartItemId, int quantity)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                await _cartService.UpdateCartItemAsync(user.Id, cartItemId, quantity);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int cartItemId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                await _cartService.RemoveFromCartAsync(user.Id, cartItemId);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostClearCartAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                await _cartService.ClearCartAsync(user.Id);
            }
            return RedirectToPage();
        }
    }
}
