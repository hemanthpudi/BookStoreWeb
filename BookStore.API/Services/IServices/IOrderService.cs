using BookStore.API.DTOs;
using BookStore.API.Models;

namespace BookStore.API.Services.IServices
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(string userId);
        Task<OrderDto?> GetOrderByIdAsync(string userId, int orderId);
        Task<IEnumerable<OrderDto>>GetOrdersAsync(string userId);
        Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
    }
}
