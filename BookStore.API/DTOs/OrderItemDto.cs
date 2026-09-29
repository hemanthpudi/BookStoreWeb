namespace BookStore.API.DTOs
{
    public class OrderItemDto
    {
        public int BookId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice =>
            UnitPrice * Quantity;
    }
}