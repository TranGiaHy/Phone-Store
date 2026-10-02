using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneStore.Web.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên điện thoại không được để trống")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        [StringLength(50)]
        public string? Storage { get; set; } // VD: 128GB, 256GB

        [StringLength(50)]
        public string? RAM { get; set; } // VD: 8GB, 12GB

        [StringLength(50)]
        public string? Color { get; set; } // VD: Titan Tự Nhiên, Đen

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Khóa ngoại liên kết tới Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}