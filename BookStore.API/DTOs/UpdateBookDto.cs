using System.ComponentModel.DataAnnotations;

namespace BookStore.API.DTOs
{
    public class UpdateBookDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Author { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(20)]
        public string? ISBN { get; set; }

        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}