using System.ComponentModel.DataAnnotations;

namespace BookStore.API.DTOs
{
    public class CartItemRequestDto
    {
        [Range(1, int.MaxValue)]
        public int BookId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}