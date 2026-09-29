using BookStore.API.DTOs;
using BookStore.API.Models;
using BookStore.API.Repositories.IRepositories;
using BookStore.API.Services.ServiceImpl;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookStore.API.Tests.Services
{
    public class CartServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<CartService>> _loggerMock;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<CartService>>();

            _cartService = new CartService(
                _unitOfWorkMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task AddToCartAsync_WhenQuantityIsZero_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = "user-1";

            var request = new CartItemRequestDto
            {
                BookId = 1,
                Quantity = 0
            };

            // Act
            Func<Task> act = async () =>
                await _cartService.AddToCartAsync(
                    userId,
                    request);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("Quantity must be greater than zero.");
        }

        [Fact]
        public async Task AddToCartAsync_WhenValidRequest_ShouldAddBookToCart()
        {
            // Arrange
            var userId = "user-1";

            var request = new CartItemRequestDto
            {
                BookId = 1,
                Quantity = 2
            };

            var book = new Book
            {
                Id = 1,
                Title = "Clean Code",
                Price = 450,
                StockQuantity = 10
            };

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = new List<CartItem>()
            };

            var updatedCart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = new List<CartItem>
        {
            new CartItem
            {
                BookId = 1,
                Quantity = 2,
                Book = book
            }
        }
            };

            _unitOfWorkMock
                .Setup(x => x.Books.GetBookByIdAsync(1))
                .ReturnsAsync(book);

            _unitOfWorkMock
                .Setup(x => x.Carts.GetCartByUserIdAsync(userId))
                .ReturnsAsync(cart);

            _unitOfWorkMock
                .Setup(x => x.Carts.GetCartItemAsync(1, 1))
                .ReturnsAsync((CartItem?)null);

            _unitOfWorkMock
                .Setup(x => x.Carts.GetCartByUserIdAsync(userId))
                .ReturnsAsync(updatedCart);

            // Act
            var result = await _cartService.AddToCartAsync(
                userId,
                request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.First().BookId.Should().Be(1);
            result.Items.First().Quantity.Should().Be(2);

            _unitOfWorkMock.Verify(
                x => x.Carts.AddCartItemAsync(
                    It.Is<CartItem>(item =>
                        item.BookId == 1 &&
                        item.Quantity == 2)),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task AddToCartAsync_WhenBookDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var userId = "user-1";

            var request = new CartItemRequestDto
            {
                BookId = 999,
                Quantity = 1
            };

            _unitOfWorkMock
                .Setup(x => x.Books.GetBookByIdAsync(999))
                .ReturnsAsync((Book?)null);

            // Act
            Func<Task> act = async () =>
                await _cartService.AddToCartAsync(
                    userId,
                    request);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Book not found.");
        }

        [Fact]
        public async Task AddToCartAsync_WhenQuantityExceedsStock_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = "user-1";

            var request = new CartItemRequestDto
            {
                BookId = 1,
                Quantity = 10
            };

            var book = new Book
            {
                Id = 1,
                Title = "Clean Code",
                Price = 450,
                StockQuantity = 5
            };

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = new List<CartItem>()
            };

            _unitOfWorkMock
                .Setup(x => x.Books.GetBookByIdAsync(1))
                .ReturnsAsync(book);

            _unitOfWorkMock
                .Setup(x => x.Carts.GetCartByUserIdAsync(userId))
                .ReturnsAsync(cart);

            _unitOfWorkMock
                .Setup(x => x.Carts.GetCartItemAsync(1, 1))
                .ReturnsAsync((CartItem?)null);

            // Act
            Func<Task> act = async () =>
                await _cartService.AddToCartAsync(
                    userId,
                    request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Requested quantity exceeds available stock.");
        }
    }
}