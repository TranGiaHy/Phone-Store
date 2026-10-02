using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Web.Data;

namespace PhoneStore.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Details(int id)
        {
            // Tìm sản phẩm theo id (nhớ Include Category nếu bạn có dùng Navigation Property)
            var product = _context.Products
                // .Include(p => p.Category) // Mở comment dòng này nếu Category bị null trên View
                .FirstOrDefault(p => p.Id == id);

            // Nếu id không tồn tại, trả về trang 404 chuẩn
            if (product == null)
            {
                return NotFound(); 
            }

            // Truyền sản phẩm ra View
            return View(product);
        }
        
        // Trang chủ: Duyệt danh sách + Tìm kiếm + Lọc theo hãng & giá
        public async Task<IActionResult> Index(string searchString, int? categoryId, string priceRange)
        {
            // 1. Load danh sách hãng (Category) truyền ra View để hiện ở khung chọn
            ViewBag.Categories = await _context.Categories.ToListAsync();
            
            // 2. Lưu lại các giá trị người dùng vừa chọn để giữ nguyên trạng thái ô chọn
            ViewBag.SearchString = searchString;
            ViewBag.PriceRange = priceRange;
            ViewBag.CategoryId = categoryId;

            // 3. Lấy toàn bộ sản phẩm
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // 4. Xử lý Lọc theo Tên máy
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            // 5. Xử lý Lọc theo Hãng
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // 6. Xử lý Lọc theo Khoảng giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "under15": // Dưới 15 triệu
                        products = products.Where(p => p.Price < 15000000);
                        break;
                    case "15to25": // Từ 15 đến 25 triệu
                        products = products.Where(p => p.Price >= 15000000 && p.Price <= 25000000);
                        break;
                    case "over25": // Trên 25 triệu
                        products = products.Where(p => p.Price > 25000000);
                        break;
                }
            }

            // 7. Sắp xếp sản phẩm mới nhất lên đầu và trả về View
            return View(await products.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}