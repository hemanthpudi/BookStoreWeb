using BookStore.API.DTOs;
using BookStore.API.Models;
using BookStore.API.Repositories;
using BookStore.API.Repositories.IRepositories;
using BookStore.API.Services.IServices;

namespace BookStore.API.Services.ServiceImpl
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CartService> _logger;

        public CartService(IUnitOfWork unitOfWork,
                           ILogger<CartService> logger)
        {
           _unitOfWork = unitOfWork;
           _logger = logger;
        }
        

        public async Task<CartDto> AddToCartAsync(string userId,CartItemRequestDto request)
        {
            //Add Logging for adding item to cart
            _logger.LogInformation(
                "User {UserId} is adding book {BookId} with quantity {Quantity} to cart.",
                userId,
                request.BookId,
                request.Quantity);

            if (request.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }

            var book = await _unitOfWork.Books.GetBookByIdAsync(request.BookId);

            if (book == null)
            {
                throw new KeyNotFoundException(
                    "Book not found.");
            }

            var cart = await _unitOfWork.Carts
                .GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                await _unitOfWork.Carts.AddCartAsync(cart);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation(
                    "New cart created for user {UserId}.",
                     userId);
            }

            var existingItem = await _unitOfWork.Carts
                .GetCartItemAsync(
                    cart.Id,
                    request.BookId);

            var newQuantity = request.Quantity;

            if (existingItem != null)
            {
                newQuantity =
                    existingItem.Quantity + request.Quantity;
            }

            if (newQuantity > book.StockQuantity)
            {
                throw new InvalidOperationException(
                    "Requested quantity exceeds available stock.");
            }

            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;

                _unitOfWork.Carts.UpdateCartItem(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    BookId = book.Id,
                    Quantity = request.Quantity
                };

                await _unitOfWork.Carts.AddCartItemAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();

            var updatedCart = await _unitOfWork.Carts
                .GetCartByUserIdAsync(userId);

            return MapToDto(updatedCart!);
            
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            if(cart==null)
            {
                return;
            }
            _unitOfWork.Carts.RemoveCartItem(cart.CartItems);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<CartDto> GetCartAsync(string userId)
        {
            var cart = await _unitOfWork.Carts
                .GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                return new CartDto();
            }

            return MapToDto(cart);
        }

        public async Task<CartDto> UpdateCartItemAsync(
            string userId,
            int bookId,
            int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }

            var cart = await _unitOfWork.Carts
                .GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                throw new KeyNotFoundException(
                    "Cart not found.");
            }

            var cartItem = await _unitOfWork.Carts
                .GetCartItemAsync(cart.Id, bookId);

            if (cartItem == null)
            {
                throw new KeyNotFoundException(
                    "Cart item not found.");
            }

            var book = await _unitOfWork.Books
                .GetBookByIdAsync(bookId);

            if (book == null)
            {
                throw new KeyNotFoundException(
                    "Book not found.");
            }

            if (quantity > book.StockQuantity)
            {
                throw new InvalidOperationException(
                    "Requested quantity exceeds available stock.");
            }

            cartItem.Quantity = quantity;

            _unitOfWork.Carts.UpdateCartItem(cartItem);

            await _unitOfWork.SaveChangesAsync();

            var updatedCart = await _unitOfWork.Carts
                .GetCartByUserIdAsync(userId);

            return MapToDto(updatedCart!);
        }
        public async Task RemoveCartItemAsync(
            string userId,
            int bookId)
        {
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            if (cart == null)
                {
                    throw new KeyNotFoundException("Cart is not found");
                }
            var cartItem=await _unitOfWork.Carts.GetCartItemAsync(cart.Id,bookId);
            if (cartItem == null)
                {
                    throw new KeyNotFoundException("Cart item not found");
                }
            _unitOfWork.Carts.RemoveCartItem(cartItem);
            await _unitOfWork.SaveChangesAsync();
        }
        private CartDto MapToDto(Cart cart)
        {
            return new CartDto
            {
                Id = cart.Id,

                Items = cart.CartItems
                    .Select(item => new CartItemDto
                    {
                        BookId = item.BookId,
                        Title = item.Book.Title,
                        Price = item.Book.Price,
                        Quantity = item.Quantity
                    })
                    .ToList()
            };
        }
    }
}