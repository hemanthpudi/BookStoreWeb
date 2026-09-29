namespace BookStore.API.DTOs
{
    public class CartItemDto
    {
        public int BookId { get; set; }

        public string Title { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice =>
            Price * Quantity;
    }
}