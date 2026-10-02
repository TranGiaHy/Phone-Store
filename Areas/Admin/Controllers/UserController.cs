using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Web.Models;

namespace PhoneStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. Danh sách tài khoản
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "Customer";
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // 2. Thêm tài khoản mới (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 2. Thêm tài khoản mới (POST)
        [HttpPost]
        public async Task<IActionResult> Create(string email, string phoneNumber, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ Email và Mật khẩu!";
                return View();
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["Error"] = "Email này đã tồn tại trong hệ thống!";
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = phoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                string selectedRole = string.IsNullOrEmpty(role) ? "Customer" : role;
                if (!await _roleManager.RoleExistsAsync(selectedRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(selectedRole));
                }
                await _userManager.AddToRoleAsync(user, selectedRole);

                TempData["Success"] = $"Đã tạo tài khoản {email} thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            return View();
        }

        // 3. Sửa tài khoản & Đặt lại mật khẩu (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.CurrentRole = roles.FirstOrDefault() ?? "Customer";
            return View(user);
        }

        // 3. Sửa tài khoản & Đặt lại mật khẩu (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(string id, string email, string phoneNumber, string role, string? newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Email = email;
            user.UserName = email;
            user.PhoneNumber = phoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                TempData["Error"] = "Lỗi khi cập nhật thông tin!";
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật quyền (Role)
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            string selectedRole = string.IsNullOrEmpty(role) ? "Customer" : role;
            if (!await _roleManager.RoleExistsAsync(selectedRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(selectedRole));
            }
            await _userManager.AddToRoleAsync(user, selectedRole);

            // Nếu Admin có nhập mật khẩu mới -> Cấp lại mật khẩu cho User
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!passResult.Succeeded)
                {
                    TempData["Error"] = "Không thể đổi mật khẩu: " + string.Join(", ", passResult.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Edit), new { id = user.Id });
                }
            }

            TempData["Success"] = $"Đã cập nhật tài khoản {user.Email}!";
            return RedirectToAction(nameof(Index));
        }

        // 4. Khóa / Mở khóa tài khoản
        [HttpPost]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.UserName == User.Identity?.Name)
            {
                TempData["Error"] = "Bạn không thể tự khóa tài khoản đang đăng nhập của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                // Đang bị khóa -> Mở khóa
                user.LockoutEnd = null;
                TempData["Success"] = $"Đã mở khóa tài khoản {user.Email}!";
            }
            else
            {
                // Đang hoạt động -> Khóa 100 năm
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
                TempData["Success"] = $"Đã khóa tài khoản {user.Email}!";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        // 5. Xóa tài khoản
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (user.UserName == User.Identity?.Name)
                {
                    TempData["Error"] = "Bạn không thể tự xóa tài khoản Admin đang đăng nhập!";
                    return RedirectToAction(nameof(Index));
                }

                await _userManager.DeleteAsync(user);
                TempData["Success"] = $"Đã xóa tài khoản {user.Email}!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}