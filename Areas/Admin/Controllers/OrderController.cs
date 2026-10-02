using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Web.Data;

namespace PhoneStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int orderId, string paymentStatus, string orderStatus)
        {
            int targetId = id > 0 ? id : orderId;
            var order = await _context.Orders.FindAsync(targetId);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng cần cập nhật!";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(paymentStatus)) order.PaymentStatus = paymentStatus;
            if (!string.IsNullOrEmpty(orderStatus)) order.OrderStatus = orderStatus;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật trạng thái đơn hàng #{order.Id} thành công!";
            return RedirectToAction(nameof(Index));
        }

        // TÍNH NĂNG XÓA ĐƠN HÀNG
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                // Tự động hoàn lại hàng vào kho nếu xóa đơn chưa hủy
                if (order.OrderStatus != "Cancelled")
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        var product = await _context.Products.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += detail.Quantity;
                        }
                    }
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa vĩnh viễn đơn hàng #{id} và hoàn lại sản phẩm vào kho!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}