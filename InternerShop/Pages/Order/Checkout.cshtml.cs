using InternerShop.Data;
using InternerShop.Models;
using InternerShop.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Pages.Order
{
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CartService _cartService;

        public CheckoutModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        [BindProperty]
        public OrderInputModel OrderInput { get; set; } = new OrderInputModel();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Identity/Account/Login", new { returnUrl = "/Order/Checkout" });
            }

            var user = await _userManager.GetUserAsync(User);
            var cart = await _cartService.GetOrCreateCartAsync(user.Id);
            CartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .Include(ci => ci.Product)
                .ToListAsync();

            if (!CartItems.Any())
            {
                return RedirectToPage("/Cart");
            }

            // Заполняем данные пользователя
            OrderInput.FirstName = user.FirstName;
            OrderInput.LastName = user.LastName;
            OrderInput.Email = user.Email;
            OrderInput.Phone = user.PhoneNumber;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Identity/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var cart = await _cartService.GetOrCreateCartAsync(user.Id);
                CartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .Include(ci => ci.Product)
                    .ToListAsync();
                return Page();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var userCart = await _cartService.GetOrCreateCartAsync(currentUser.Id);
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == userCart.CartId)
                .Include(ci => ci.Product)
                .ToListAsync();

            if (!cartItems.Any())
            {
                ModelState.AddModelError("", "Ваша корзина пуста.");
                return Page();
            }

            // Создаем заказ
            var order = new Models.Order
            {
                UserId = currentUser.Id,
                TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price),
                OrderStatus = "Pending",
                OrderDate = DateTime.UtcNow,
                ShippingNotes = OrderInput.Notes
            };

            // Создаем временный адрес доставки (упрощенная версия)
            var tempAddress = new UserAddress
            {
                UserId = currentUser.Id,
                AddressType = "Shipping",
                HouseNumber = "1",
                PostalCode = OrderInput.PostalCode
            };

            // Создаем связанные сущности для адреса
            var country = new Country { Name = OrderInput.Country };
            var region = new Region { Name = OrderInput.City, Country = country };
            var city = new City { Name = OrderInput.City, Region = region };
            var street = new Street { Name = OrderInput.Address, City = city };

            tempAddress.Street = street;

            _context.UserAddresses.Add(tempAddress);
            await _context.SaveChangesAsync();

            order.ShippingAddressId = tempAddress.UserAddressId;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Добавляем товары в заказ
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.Price
                };
                _context.OrderItems.Add(orderItem);

                // Обновляем количество товара на складе
                cartItem.Product.StockQuantity -= cartItem.Quantity;
                cartItem.Product.UpdatedDate = DateTime.UtcNow;
            }

            // Очищаем корзину
            await _cartService.ClearCartAsync(currentUser.Id);

            await _context.SaveChangesAsync();

            return RedirectToPage("/Order/Confirmation", new { orderId = order.OrderId });
        }
    }

    public class OrderInputModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(100, ErrorMessage = "Фамилия не должна превышать 100 символов")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Адрес обязателен")]
        [StringLength(500, ErrorMessage = "Адрес не должен превышать 500 символов")]
        [Display(Name = "Адрес доставки")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Город обязателен")]
        [StringLength(100, ErrorMessage = "Город не должен превышать 100 символов")]
        [Display(Name = "Город")]
        public string City { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Почтовый индекс не должен превышать 20 символов")]
        [Display(Name = "Почтовый индекс")]
        public string? PostalCode { get; set; }

        [Required(ErrorMessage = "Страна обязательна")]
        [StringLength(100, ErrorMessage = "Страна не должна превышать 100 символов")]
        [Display(Name = "Страна")]
        public string Country { get; set; } = "Россия";

        [StringLength(1000, ErrorMessage = "Примечания не должны превышать 1000 символов")]
        [Display(Name = "Примечания к заказу")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Необходимо согласие с условиями")]
        [Display(Name = "Согласие с условиями")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Необходимо согласие с условиями")]
        public bool AgreeToTerms { get; set; }
    }
}
