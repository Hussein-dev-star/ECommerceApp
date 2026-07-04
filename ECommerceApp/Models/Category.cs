using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models

{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم القسم مطلوب")]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        public ICollection<Product> Products { get; set; } = new List<Product>();

    }


}
