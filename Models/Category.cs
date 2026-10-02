using System.ComponentModel.DataAnnotations;

namespace PhoneStore.Web.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên hãng không được để trống")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation property: 1 hãng có nhiều điện thoại
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}