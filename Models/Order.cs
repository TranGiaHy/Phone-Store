using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneStore.Web.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Trạng thái đơn: Pending (Chờ xử lý), Processing (Đang xử lý), Shipping (Đang giao), Delivered (Đã giao), Cancelled (Đã hủy)
        public string OrderStatus { get; set; } = "Pending";

        // Phương thức: COD, Visa/MasterCard (Mock)
        public string PaymentMethod { get; set; } = "COD";

        // Trạng thái thanh toán: Unpaid, Paid
        public string PaymentStatus { get; set; } = "Unpaid";

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}