using System.ComponentModel.DataAnnotations;

namespace BookStore.API.DTOs
{
    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}