using BookStore.API.Models;

namespace BookStore.API.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByUserIdAsync(string userId);

        Task<CartItem?> GetCartItemAsync(
            int cartId,
            int bookId);

        Task AddCartAsync(Cart cart);

        Task AddCartItemAsync(CartItem cartItem);

        void UpdateCartItem(CartItem cartItem);

        void RemoveCartItem(CartItem cartItem);
        void RemoveCartItem(IEnumerable<CartItem> cartItem);
        void RemoveCart(Cart cart);

    }
}