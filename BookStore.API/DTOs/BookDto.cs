using System.ComponentModel.DataAnnotations;

namespace BookStore.API.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        [Required]
        [StringLength(150)]
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ISBN { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}