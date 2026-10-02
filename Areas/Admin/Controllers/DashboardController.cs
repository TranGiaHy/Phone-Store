using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Web.Data;

namespace PhoneStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Tính tổng doanh thu 
            ViewBag.TotalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);

            // 2. Đếm tổng số đơn hàng
            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            // 3. Đếm tổng số mẫu điện thoại đang bán
            ViewBag.TotalProducts = await _context.Products.CountAsync();

            // 4. Lấy danh sách 5 đơn hàng mới nhất để hiện ở bảng
            ViewBag.RecentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}