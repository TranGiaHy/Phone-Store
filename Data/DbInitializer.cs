using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Web.Models;

namespace PhoneStore.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Tự động chạy Migration nếu DB chưa tồn tại
            await context.Database.MigrateAsync();

            // 1. Tạo Roles (Admin & Customer)
            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Tạo tài khoản Admin mặc định
            var adminEmail = "admin@phonestore.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Quản Trị Viên Hệ Thống",
                    Address = "TP. Hồ Chí Minh",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Tạo dữ liệu Hãng & Điện thoại mẫu nếu bảng đang trống
            if (!await context.Categories.AnyAsync())
            {
                var apple = new Category { Name = "Apple (iPhone)", Description = "Điện thoại iPhone chính hãng VN/A" };
                var samsung = new Category { Name = "Samsung", Description = "Điện thoại Samsung Galaxy chính hãng" };
                var xiaomi = new Category { Name = "Xiaomi", Description = "Điện thoại Xiaomi cấu hình cao" };

                context.Categories.AddRange(apple, samsung, xiaomi);
                await context.SaveChangesAsync();

                context.Products.AddRange(
                    new Product
                    {
                        Name = "iPhone 16 Pro Max",
                        CategoryId = apple.Id,
                        Price = 33990000,
                        StockQuantity = 20,
                        RAM = "8GB",
                        Storage = "256GB",
                        Color = "Titan Sa Mạc",
                        ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/329149/iphone-16-pro-max-sa-mac-thumb-600x600.jpg",
                        Description = "Chip A18 Pro mạnh mẽ, nút điều khiển Camera chuyên dụng."
                    },
                    new Product
                    {
                        Name = "Samsung Galaxy S24 Ultra",
                        CategoryId = samsung.Id,
                        Price = 27990000,
                        StockQuantity = 15,
                        RAM = "12GB",
                        Storage = "256GB",
                        Color = "Xám Titan",
                        ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/307174/samsung-galaxy-s24-ultra-grey-thumbnew-600x600.jpg",
                        Description = "Tích hợp quyền năng Galaxy AI, bút S-Pen và camera 200MP."
                    },
                    new Product
                    {
                        Name = "Xiaomi 14 Ultra 5G",
                        CategoryId = xiaomi.Id,
                        Price = 24990000,
                        StockQuantity = 10,
                        RAM = "16GB",
                        Storage = "512GB",
                        Color = "Đen",
                        ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/313889/xiaomi-14-ultra-black-thumbnew-600x600.jpg",
                        Description = "Ống kính quang học Leica thế hệ mới, Snapdragon 8 Gen 3."
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}