using InternerShop.Data;
using InternerShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Pages.Admin.News
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        [BindProperty]
        public NewsCreateModel NewsCreate { get; set; } = new NewsCreateModel();

        [BindProperty]
        public IFormFile? NewsImage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToPage("/Identity/Account/Login");
                }

                var news = new Models.News
                {
                    Title = NewsCreate.Title,
                    Content = NewsCreate.Content,
                    IsPublished = NewsCreate.IsPublished,
                    AuthorId = user.Id,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                // Обработка изображения
                if (NewsImage != null && NewsImage.Length > 0)
                {
                    news.ImageUrl = await SaveImageAsync(NewsImage, "news");
                }
                else
                {
                    news.ImageUrl = NewsCreate.ImageUrl; // Используем URL если файл не загружен
                }

                if (NewsCreate.IsPublished && news.PublishedDate == default)
                {
                    news.PublishedDate = DateTime.UtcNow;
                }

                _context.News.Add(news);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Новость успешно создана";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при создании новости: {ex.Message}");
                return Page();
            }
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile, string folderName)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", folderName);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/{folderName}/{fileName}";
        }
    }

    public class NewsCreateModel
    {
        [Required(ErrorMessage = "Заголовок обязателен")]
        [StringLength(200, ErrorMessage = "Заголовок не должен превышать 200 символов")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Содержание обязательно")]
        public string Content { get; set; } = string.Empty;

        [Url(ErrorMessage = "Введите корректный URL изображения")]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsPublished { get; set; } = false;
    }
}
