namespace BookStore.API.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }

        public List<CartItemDto> Items { get; set; }
            = new();

        public decimal TotalAmount =>
            Items.Sum(item => item.TotalPrice);
    }
}