using BookStore.API.Models;
using BookStore.API.Repositories.IRepositories;
using BookStore.API.Services.ServiceImpl;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookStore.API.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _orderService = new OrderService(
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenCartDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var userId = "user-1";

            _unitOfWorkMock
                .Setup(x => x.Carts.GetCartByUserIdAsync(userId))
                .ReturnsAsync((Cart?)null);

            // Act
            Func<Task> act = async () =>
                await _orderService.CreateOrderAsync(userId);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Cart not found.");
        }

        [Fact]
        public async Task CreateOrderAsync_WhenCartIsEmpty_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = "user-1";

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = new List<CartItem>()
            };

            _unitOfWorkMock
                .Setup(x => x.Carts.GetCartByUserIdAsync(userId))
                .ReturnsAsync(cart);

            // Act
            Func<Task> act = async () =>
                await _orderService.CreateOrderAsync(userId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot create an order from an empty cart.");
        }

        [Fact]
        public async Task CreateOrderAsync_WhenCartHasValidItems_ShouldCreateOrder()
        {
            // Arrange
            var userId = "user-1";

            var book = new Book
            {
                Id = 1,
                Title = "Clean Code",
                Price = 450,
                StockQuantity = 10
            };

            var cartItem = new CartItem
            {
                BookId = 1,
                Quantity = 2,
                Book = book
            };

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = new List<CartItem>
        {
            cartItem
        }
            };

            _unitOfWorkMock
                .Setup(x => x.Carts.GetCartByUserIdAsync(userId))
                .ReturnsAsync(cart);

            _unitOfWorkMock
                .Setup(x => x.Orders.AddOrderAsync(
                    It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.Orders.AddOrderItemAsync(
                    It.IsAny<OrderItem>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.Carts.RemoveCartItem(
                    It.IsAny<IEnumerable<CartItem>>()));

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(
                    It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(action => action());

            // Act
            var result = await _orderService.CreateOrderAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.TotalAmount.Should().Be(900);
            result.Status.Should().Be(OrderStatus.Pending.ToString());

            book.StockQuantity.Should().Be(8);

            _unitOfWorkMock.Verify(
                x => x.Orders.AddOrderAsync(
                    It.Is<Order>(o =>
                        o.UserId == userId &&
                        o.Status == OrderStatus.Pending &&
                        o.TotalAmount == 900)),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.Orders.AddOrderItemAsync(
                    It.Is<OrderItem>(item =>
                        item.BookId == 1 &&
                        item.Quantity == 2 &&
                        item.UnitPrice == 450)),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.Carts.RemoveCartItem(
                    It.IsAny<IEnumerable<CartItem>>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.ExecuteInTransactionAsync(
                    It.IsAny<Func<Task>>()),
                Times.Once);
        }
        [Fact]
        public async Task CreateOrderAsync_WhenStockIsInsufficient_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = "user-1";

            var book = new Book
            {
                Id = 1,
                Title = "Clean Code",
                Price = 450,
                StockQuantity = 1
            };

            var cartItem = new CartItem
            {
                BookId = 1,
                Quantity = 2,
                Book = book
            };

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = new List<CartItem>
        {
            cartItem
        }
            };

            _unitOfWorkMock
                .Setup(x => x.Carts.GetCartByUserIdAsync(userId))
                .ReturnsAsync(cart);

            _unitOfWorkMock
                .Setup(x => x.Orders.AddOrderAsync(
                    It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.ExecuteInTransactionAsync(
                    It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(action => action());

            // Act
            Func<Task> act = async () =>
                await _orderService.CreateOrderAsync(userId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Insufficient stock for book 'Clean Code'.");

            // Verify that no order item was created
            _unitOfWorkMock.Verify(
                x => x.Orders.AddOrderItemAsync(
                    It.IsAny<OrderItem>()),
                Times.Never);

            // Verify that cart items were not removed
            _unitOfWorkMock.Verify(
                x => x.Carts.RemoveCartItem(
                    It.IsAny<IEnumerable<CartItem>>()),
                Times.Never);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WhenOrderBelongsToAnotherUser_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var currentUserId = "user-1";

            var order = new Order
            {
                Id = 1,
                UserId = "user-2",
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = 900,
                OrderItems = new List<OrderItem>()
            };

            _unitOfWorkMock
                .Setup(x => x.Orders.GetOrderByIdAsync(1))
                .ReturnsAsync(order);

            // Act
            Func<Task> act = async () =>
                await _orderService.GetOrderByIdAsync(
                    currentUserId,
                    1);

            // Assert
            await act.Should()
                .ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("You are not allowed to access this order.");
        }

        [Fact]
        public async Task GetOrderByIdAsync_WhenOrderBelongsToUser_ShouldReturnOrder()
        {
            // Arrange
            var userId = "user-1";

            var book = new Book
            {
                Id = 1,
                Title = "Clean Code",
                Price = 450
            };

            var order = new Order
            {
                Id = 1,
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = 900,
                OrderItems = new List<OrderItem>
        {
            new OrderItem
            {
                BookId = 1,
                Quantity = 2,
                UnitPrice = 450,
                Book = book
            }
        }
            };

            _unitOfWorkMock
                .Setup(x => x.Orders.GetOrderByIdAsync(1))
                .ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderByIdAsync(
                userId,
                1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Status.Should().Be(OrderStatus.Pending.ToString());
            result.TotalAmount.Should().Be(900);

            result.Items.Should().HaveCount(1);
            result.Items.First().BookId.Should().Be(1);
            result.Items.First().Title.Should().Be("Clean Code");
            result.Items.First().Quantity.Should().Be(2);
            result.Items.First().UnitPrice.Should().Be(450);
        }

        [Fact]
        public async Task GetOrdersAsync_WhenUserHasOrders_ShouldReturnOrders()
        {
            // Arrange
            var userId = "user-1";

            var orders = new List<Order>
    {
        new Order
        {
            Id = 1,
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = 900,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    BookId = 1,
                    Quantity = 2,
                    UnitPrice = 450,
                    Book = new Book
                    {
                        Id = 1,
                        Title = "Clean Code",
                        Price = 450
                    }
                }
            }
        },

        new Order
        {
            Id = 2,
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Confirmed,
            TotalAmount = 500,
            OrderItems = new List<OrderItem>()
        }
    };

            _unitOfWorkMock
                .Setup(x => x.Orders.GetOrderByUserIdAsync(userId))
                .ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetOrdersAsync(userId);

            // Assert
            result.Should().NotBeNull();

            var orderList = result.ToList();

            orderList.Should().HaveCount(2);

            orderList[0].Id.Should().Be(1);
            orderList[0].TotalAmount.Should().Be(900);
            orderList[0].Status.Should().Be(OrderStatus.Pending.ToString());

            orderList[1].Id.Should().Be(2);
            orderList[1].TotalAmount.Should().Be(500);
            orderList[1].Status.Should().Be(OrderStatus.Confirmed.ToString());

            _unitOfWorkMock.Verify(
                x => x.Orders.GetOrderByUserIdAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task GetOrdersAsync_WhenUserHasNoOrders_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = "user-1";

            _unitOfWorkMock
                .Setup(x => x.Orders.GetOrderByUserIdAsync(userId))
                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _orderService.GetOrdersAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
