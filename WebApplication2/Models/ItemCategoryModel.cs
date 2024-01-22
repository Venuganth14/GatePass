using System.ComponentModel.DataAnnotations;

namespace GatePass_Project.Models
{
    public class ItemCategoryModel
    {
        [Key]
        public int Category_id { get; set; }
        [Required]
        public string Category_name { get; set; }
        [Required]
        public string Category_type { get; set; }
        public DateTime? Created_date { get; set; }
        public DateTime? Removed_date { get; set; }
    }
}
