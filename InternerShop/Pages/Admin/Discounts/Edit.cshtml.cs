using InternerShop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Pages.Admin.Discounts
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EditModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public DiscountEditModel DiscountEdit { get; set; } = new DiscountEditModel();

        [BindProperty]
        public IFormFile? DiscountImage { get; set; }

        public string? CurrentImageUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }

            DiscountEdit = new DiscountEditModel
            {
                DiscountId = discount.DiscountId,
                Name = discount.Name,
                Description = discount.Description,
                DiscountPercentage = discount.DiscountPercentage,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                IsActive = discount.IsActive
            };

            CurrentImageUrl = discount.ImageUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var existingDiscount = await _context.Discounts.FindAsync(DiscountEdit.DiscountId);
                if (existingDiscount == null)
                {
                    return NotFound();
                }

                existingDiscount.Name = DiscountEdit.Name;
                existingDiscount.Description = DiscountEdit.Description;
                existingDiscount.DiscountPercentage = DiscountEdit.DiscountPercentage;
                existingDiscount.StartDate = DiscountEdit.StartDate;
                existingDiscount.EndDate = DiscountEdit.EndDate;
                existingDiscount.IsActive = DiscountEdit.IsActive;
                existingDiscount.UpdatedDate = DateTime.UtcNow;

                // Обработка нового изображения
                if (DiscountImage != null && DiscountImage.Length > 0)
                {
                    existingDiscount.ImageUrl = await SaveImageAsync(DiscountImage, "discounts");
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Скидка успешно обновлена";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при обновлении скидки: {ex.Message}");
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

    public class DiscountEditModel
    {
        public int DiscountId { get; set; }

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
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Дата окончания обязательна")]
        [Display(Name = "Дата окончания")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}