using System.ComponentModel.DataAnnotations;

namespace BookStore.API.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}