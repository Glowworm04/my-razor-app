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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EditModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Используем отдельную модель для привязки
        [BindProperty]
        public ProductEditModel ProductEdit { get; set; } = new ProductEditModel();

        [BindProperty]
        public IFormFileCollection NewProductImages { get; set; } = new FormFileCollection();

        [BindProperty]
        public List<int> DeleteImages { get; set; } = new List<int>();

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public List<ProductImage> ExistingImages { get; set; } = new List<ProductImage>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await LoadCategoriesAsync();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Заполняем модель редактирования
            ProductEdit = new ProductEditModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive
            };

            await LoadExistingImagesAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadCategoriesAsync(); // ВАЖНО: загружаем категории ДО проверки валидации
            await LoadExistingImagesAsync(ProductEdit.ProductId);

            if (!ModelState.IsValid)
            {
                // Показываем ошибки
                return Page();
            }

            try
            {
                var existingProduct = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == ProductEdit.ProductId);

                if (existingProduct == null)
                {
                    ModelState.AddModelError("", "Товар не найден");
                    return Page();
                }

                // Обновляем данные
                existingProduct.Name = ProductEdit.Name;
                existingProduct.Description = ProductEdit.Description;
                existingProduct.Price = ProductEdit.Price;
                existingProduct.StockQuantity = ProductEdit.StockQuantity;
                existingProduct.CategoryId = ProductEdit.CategoryId;
                existingProduct.IsActive = ProductEdit.IsActive;
                existingProduct.UpdatedDate = DateTime.UtcNow;

                // Обработка изображений
                await ProcessImageChangesAsync(existingProduct);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Товар успешно обновлен";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка: {ex.Message}");
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

        private async Task LoadExistingImagesAsync(int productId)
        {
            ExistingImages = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();
        }

        private async Task ProcessImageChangesAsync(Product product)
        {
            // Удаление изображений
            if (DeleteImages.Any())
            {
                var imagesToDelete = product.ProductImages
                    .Where(pi => DeleteImages.Contains(pi.ProductImageId))
                    .ToList();

                foreach (var image in imagesToDelete)
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    _context.ProductImages.Remove(image);
                }
            }

            // Добавление новых изображений
            if (NewProductImages?.Count > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                bool hasPrimaryImage = product.ProductImages.Any(pi => pi.IsPrimary);

                foreach (var file in NewProductImages)
                {
                    if (file.Length > 0 && file.Length < 5 * 1024 * 1024)
                    {
                        var extension = Path.GetExtension(file.FileName).ToLower();
                        if (new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension))
                        {
                            var fileName = Guid.NewGuid() + extension;
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                                await file.CopyToAsync(stream);

                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.ProductId,
                                ImageUrl = $"/images/products/{fileName}",
                                AltText = product.Name,
                                IsPrimary = !hasPrimaryImage
                            });

                            hasPrimaryImage = true;
                        }
                    }
                }
            }
        }
    }

    // Модель для редактирования товара
    public class ProductEditModel
    {
        public int ProductId { get; set; }

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
