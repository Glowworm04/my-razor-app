using InternerShop.Data;
using InternerShop.Models;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ShoppingCart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart { UserId = userId };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task AddToCartAsync(string userId, int productId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            cart.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(string userId, int cartItemId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);

            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }

                cart.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(string userId, int cartItemId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                cart.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            _context.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
        }

        public async Task<int> GetCartItemsCountAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return cart.CartItems.Sum(ci => ci.Quantity);
        }
    }
}
