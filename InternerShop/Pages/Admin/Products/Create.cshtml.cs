using InternerShop.Data;
using InternerShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InternerShop.Pages.Admin.Products
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

        // Используем отдельную модель для привязки
        [BindProperty]
        public ProductCreateModel ProductCreate { get; set; } = new ProductCreateModel();

        [BindProperty]
        public IFormFileCollection ProductImages { get; set; } = new FormFileCollection();

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCategoriesAsync(); // ВАЖНО: загружаем категории ДО проверки валидации

            if (!ModelState.IsValid)
            {
                // Показываем ошибки
                return Page();
            }

            try
            {
                // Создаем новый товар
                var product = new Product
                {
                    Name = ProductCreate.Name,
                    Description = ProductCreate.Description,
                    Price = ProductCreate.Price,
                    StockQuantity = ProductCreate.StockQuantity,
                    CategoryId = ProductCreate.CategoryId,
                    IsActive = ProductCreate.IsActive,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync(); // Сохраняем чтобы получить ProductId

                // Обрабатываем изображения
                if (ProductImages != null && ProductImages.Count > 0)
                {
                    await ProcessImagesAsync(product);
                }

                TempData["SuccessMessage"] = "Товар успешно создан";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при создании товара: {ex.Message}");
                return Page();
            }
        }

        private async Task LoadCategoriesAsync()
        {
            Categories = await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        private async Task ProcessImagesAsync(Product product)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            bool isFirstImage = true;

            foreach (var file in ProductImages)
            {
                if (file.Length > 0 && file.Length < 5 * 1024 * 1024) // Максимум 5MB
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(file.FileName).ToLower();

                    if (allowedExtensions.Contains(extension))
                    {
                        var fileName = Guid.NewGuid().ToString() + extension;
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Сохраняем файл
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Создаем запись в базе данных
                        var productImage = new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = $"/images/products/{fileName}",
                            AltText = product.Name,
                            IsPrimary = isFirstImage
                        };

                        _context.ProductImages.Add(productImage);
                        isFirstImage = false;
                    }
                }
            }
            await _context.SaveChangesAsync();
        }
    }

    // Модель для создания товара
    public class ProductCreateModel
    {
        [Required(ErrorMessage = "Название товара обязательно")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Количество на складе обязательно")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Категория обязательна")]
        [Display(Name = "Категория")]
        public int CategoryId { get; set; }
    }
}