using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المنتج إلزامي")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000, ErrorMessage = "السعر يجب أن يكون قيمة إيجابية")]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = "/images/default-product.png";

        [Display(Name = "الكمية المتاحة")]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
    }
}