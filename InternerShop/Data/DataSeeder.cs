using InternerShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InternerShop.Data
{
    public class DataSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // Создание ролей
                string[] roleNames = { "Admin", "Client" };
                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Создание администратора
                string adminEmail = "admin@shop.com";
                string adminPassword = "Admin123!";

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "User",
                        RegistrationDate = DateTime.UtcNow
                    };

                    var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }

                // Добавление тестовых категорий
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Электроника", Description = "Смартфоны, ноутбуки, планшеты" },
                        new Category { Name = "Одежда", Description = "Мужская и женская одежда" },
                        new Category { Name = "Книги", Description = "Художественная и учебная литература" },
                        new Category { Name = "Спорт", Description = "Спортивные товары и инвентарь" }
                    );
                    await context.SaveChangesAsync();
                }

                // Добавление тестовых товаров
                if (!context.Products.Any())
                {
                    var categories = await context.Categories.ToListAsync();

                    context.Products.AddRange(
                        new Product
                        {
                            Name = "Смартфон Samsung Galaxy",
                            Price = 29999.99m,
                            Description = "Современный смартфон с большим экраном",
                            StockQuantity = 50,
                            CategoryId = categories[0].CategoryId
                        },
                        new Product
                        {
                            Name = "Ноутбук HP Pavilion",
                            Price = 54999.99m,
                            Description = "Мощный ноутбук для работы и игр",
                            StockQuantity = 25,
                            CategoryId = categories[0].CategoryId
                        },
                        new Product
                        {
                            Name = "Футболка мужская",
                            Price = 1999.99m,
                            Description = "Хлопковая футболка черного цвета",
                            StockQuantity = 100,
                            CategoryId = categories[1].CategoryId
                        },
                        new Product
                        {
                            Name = "Книга 'Война и мир'",
                            Price = 899.99m,
                            Description = "Классика русской литературы",
                            StockQuantity = 30,
                            CategoryId = categories[2].CategoryId
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Добавление тестовых скидок
                if (!context.Discounts.Any())
                {
                    context.Discounts.AddRange(
                        new Discount
                        {
                            Name = "Новогодняя распродажа",
                            Description = "Скидка на все товары",
                            DiscountPercentage = 15.00m,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddDays(30)
                        },
                        new Discount
                        {
                            Name = "Скидка на электронику",
                            Description = "Специальная скидка на электронные товары",
                            DiscountPercentage = 10.00m,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddDays(60)
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Добавление тестовых новостей
                if (!context.News.Any())
                {
                    context.News.AddRange(
                        new News
                        {
                            Title = "Открытие нового магазина",
                            Content = "Мы рады сообщить об открытии нашего нового магазина в центре города!",
                            IsPublished = true,
                            PublishedDate = DateTime.UtcNow,
                            AuthorId = adminUser.Id
                        },
                        new News
                        {
                            Title = "Новая коллекция уже в продаже",
                            Content = "В нашем магазине появилась новая коллекция осенней одежды",
                            IsPublished = true,
                            PublishedDate = DateTime.UtcNow.AddDays(-1),
                            AuthorId = adminUser.Id
                        }
                    );
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
