using BookStore.API.Models;

namespace BookStore.API.Repositories.IRepositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>>GetOrderByUserIdAsync(string userId);
        Task AddOrderAsync(Order order);
        Task AddOrderItemAsync(OrderItem orderItem);
        void UpdateOrder(Order order);

    }
}
