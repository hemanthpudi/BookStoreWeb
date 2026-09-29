using BookStore.API.DTOs;

namespace BookStore.API.Services.IServices
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string userId);

        Task<CartDto> AddToCartAsync(
            string userId,
            CartItemRequestDto request);

        Task<CartDto> UpdateCartItemAsync(
            string userId,
            int bookId,
            int quantity);

        Task RemoveCartItemAsync(
            string userId,
            int bookId);

        Task ClearCartAsync(string userId);
    }
}