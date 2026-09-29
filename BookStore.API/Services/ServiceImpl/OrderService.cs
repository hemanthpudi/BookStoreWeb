using BookStore.API.DTOs;
using BookStore.API.Models;
using BookStore.API.Repositories.IRepositories;
using BookStore.API.Services.IServices;

namespace BookStore.API.Services.ServiceImpl
{
    public class OrderService:IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderDto> CreateOrderAsync(string userId)
        {
            var cart = await _unitOfWork.Carts
                .GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                throw new KeyNotFoundException("Cart not found.");
            }

            if (!cart.CartItems.Any())
            {
                throw new InvalidOperationException(
                    "Cannot create an order from an empty cart.");
            }

            Order? createdOrder = null;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    TotalAmount = 0
                };

                await _unitOfWork.Orders.AddOrderAsync(order);

                foreach (var cartItem in cart.CartItems)
                {
                    if (cartItem.Quantity >
                        cartItem.Book.StockQuantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for book '{cartItem.Book.Title}'.");
                    }

                    var orderItem = new OrderItem
                    {
                        Order = order,
                        BookId = cartItem.BookId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Book.Price
                    };

                    await _unitOfWork.Orders
                        .AddOrderItemAsync(orderItem);

                    order.TotalAmount +=
                        cartItem.Book.Price * cartItem.Quantity;

                    cartItem.Book.StockQuantity -=
                        cartItem.Quantity;
                }

                _unitOfWork.Carts.RemoveCartItem(
                    cart.CartItems);

                createdOrder = order;
            });

            return MapToDto(createdOrder!);
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,

                OrderDate = order.OrderDate,

                Status = order.Status.ToString(),

                TotalAmount = order.TotalAmount,

                Items = order.OrderItems
                    .Select(item => new OrderItemDto
                    {
                        BookId = item.BookId,

                        Title = item.Book.Title,

                        Quantity = item.Quantity,

                        UnitPrice = item.UnitPrice
                    })
                    .ToList()
            };
        }

        public async Task<OrderDto?> GetOrderByIdAsync(string userId,int orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return null;
            }

            if (order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not allowed to access this order.");
            }

            return MapToDto(order);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersAsync(string userId)
        {
            var orders = await _unitOfWork.Orders.GetOrderByUserIdAsync(userId);

            return orders
                .Select(MapToDto)
                .ToList();
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);
            if(order==null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            if (order.Status == newStatus)
            {
                throw new InvalidOperationException(
                    $"Order is already in {order.Status} status.");
            }

            if(!IsValidStatusTransition(order.Status,newStatus))
            {
                throw new InvalidOperationException(
                    $"Cannot change the order status from {order.Status} to {newStatus}");
            }
            order.Status = newStatus;
            _unitOfWork.Orders.UpdateOrder(order);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(order);

        }

        private bool IsValidStatusTransition(OrderStatus currentStatus,OrderStatus newStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending =>
                    newStatus == OrderStatus.Confirmed ||
                    newStatus == OrderStatus.Cancelled,

                OrderStatus.Confirmed =>
                    newStatus == OrderStatus.Shipped ||
                    newStatus == OrderStatus.Cancelled,

                OrderStatus.Shipped =>
                    newStatus == OrderStatus.Delivered,

                OrderStatus.Delivered =>
                    false,

                OrderStatus.Cancelled =>
                    false,

                _ => false
            };
        }
    }
}
