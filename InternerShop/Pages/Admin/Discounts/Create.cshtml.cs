using InternerShop.Data;
using InternerShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Pages.Admin.Discounts
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public DiscountCreateModel DiscountCreate { get; set; } = new DiscountCreateModel();

        [BindProperty]
        public IFormFile? DiscountImage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var discount = new Discount
                {
                    Name = DiscountCreate.Name,
                    Description = DiscountCreate.Description,
                    DiscountPercentage = DiscountCreate.DiscountPercentage,
                    StartDate = DiscountCreate.StartDate,
                    EndDate = DiscountCreate.EndDate,
                    IsActive = DiscountCreate.IsActive,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                // Обработка изображения
                if (DiscountImage != null && DiscountImage.Length > 0)
                {
                    discount.ImageUrl = await SaveImageAsync(DiscountImage, "discounts");
                }

                _context.Discounts.Add(discount);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Скидка успешно создана";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при создании скидки: {ex.Message}");
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

    public class DiscountCreateModel
    {
        [Required(ErrorMessage = "Название скидки обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Процент скидки обязателен")]
        [Range(0.01, 100, ErrorMessage = "Процент скидки должен быть от 0.01 до 100")]
        [Display(Name = "Процент скидки")]
        public decimal DiscountPercentage { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна")]
        [Display(Name = "Дата начала")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Дата окончания обязательна")]
        [Display(Name = "Дата окончания")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);

        public bool IsActive { get; set; } = true;
    }
}