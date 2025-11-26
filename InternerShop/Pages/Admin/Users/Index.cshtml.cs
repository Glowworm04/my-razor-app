using InternerShop.Data;
using InternerShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string RoleFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public DateTime? RegistrationDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalUsers { get; set; }

        public async Task OnGetAsync()
        {
            // Используем ApplicationDbContext для запроса ApplicationUser
            var usersQuery = _context.Users.OfType<ApplicationUser>().AsQueryable();

            // Фильтрация по поиску
            if (!string.IsNullOrEmpty(SearchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.FirstName.Contains(SearchString) ||
                    u.LastName.Contains(SearchString) ||
                    u.Email.Contains(SearchString));
            }

            // Фильтрация по дате регистрации
            if (RegistrationDate.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.RegistrationDate.Date == RegistrationDate.Value.Date);
            }

            // Получаем общее количество для пагинации
            TotalUsers = await usersQuery.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalUsers / (double)PageSize);

            // Получаем пользователей для текущей страницы
            var users = await usersQuery
                .OrderByDescending(u => u.RegistrationDate)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Преобразуем в ViewModel и добавляем информацию о ролях и заказах
            foreach (var user in users)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                var ordersCount = await _context.Orders.CountAsync(o => o.UserId == user.Id);

                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RegistrationDate = user.RegistrationDate,
                    IsAdmin = isAdmin,
                    OrdersCount = ordersCount
                });
            }

            // Фильтрация по роли (после загрузки ролей)
            if (!string.IsNullOrEmpty(RoleFilter))
            {
                Users = RoleFilter switch
                {
                    "Admin" => Users.Where(u => u.IsAdmin).ToList(),
                    "Client" => Users.Where(u => !u.IsAdmin).ToList(),
                    _ => Users
                };
            }
        }

        public async Task<IActionResult> OnPostToggleRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                // Убираем из роли администратора
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "Client");
            }
            else
            {
                // Добавляем в роль администратора
                await _userManager.RemoveFromRoleAsync(user, "Client");
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            TempData["SuccessMessage"] = "Роль пользователя успешно изменена";
            return RedirectToPage();
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsAdmin { get; set; }
        public int OrdersCount { get; set; }
    }
}
