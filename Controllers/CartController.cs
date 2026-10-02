using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoneStore.Web.Data;
using PhoneStore.Web.Helpers;
using PhoneStore.Web.Models;

namespace PhoneStore.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            return View(cart);
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> AddToCart(int productId, int id = 0, int quantity = 1)
        {
            int targetId = productId > 0 ? productId : id;
            var product = await _context.Products.FindAsync(targetId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == targetId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            TempData["SuccessMessage"] = $"Đã thêm \"{product.Name}\" vào giỏ hàng thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                if (quantity > 0) item.Quantity = quantity;
                else cart.Remove(item);

                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
                TempData["SuccessMessage"] = "Đã cập nhật số lượng giỏ hàng!";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int productId, int id = 0)
        {
            int targetId = productId > 0 ? productId : id;
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == targetId);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
                TempData["SuccessMessage"] = $"Đã xóa \"{item.ProductName}\" khỏi giỏ hàng!";
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            ViewBag.Cart = cart;
            ViewBag.TotalAmount = cart.Sum(i => i.Price * i.Quantity);
            return View(new Order());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            order.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            order.OrderDate = DateTime.Now;
            order.TotalAmount = cart.Sum(i => i.Price * i.Quantity);
            order.OrderStatus = "Pending";
            order.PaymentStatus = order.PaymentMethod == "COD" ? "Unpaid" : "Paid";

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };
                _context.OrderDetails.Add(detail);

                // TRỪ TỒN KHO THỰC TẾ (Cho phép rớt xuống số âm để Admin báo động nhập hàng)
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove(CartSessionKey);

            return RedirectToAction(nameof(OrderSuccess), new { id = order.Id });
        }

        [Authorize]
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}