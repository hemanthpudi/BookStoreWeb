using BookStore.API.Data;
using BookStore.API.Models;
using BookStore.API.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BookStore.API.Repositories.RepositoryImpl
{
    public class OrderRepository : IOrderRepository
    {
        private readonly BookStoreDbContext _context;
        public OrderRepository(BookStoreDbContext context)
        {
            _context = context;
        }
        public async Task AddOrderAsync(Order order)
        {
            await _context.AddAsync(order);
        }

        public async Task AddOrderItemAsync(OrderItem orderItem)
        {
            await _context.OrderItems.AddAsync(orderItem);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
           return await _context.Orders
                .Include(o=>o.OrderItems)
                .ThenInclude(oi=>oi.Book)
                .FirstOrDefaultAsync(o=>o.Id == orderId);

        }

        public async Task<IEnumerable<Order>> GetOrderByUserIdAsync(string userId)
        {
            return await _context.Orders
                 .Include(o => o.OrderItems)
                 .ThenInclude(oi => oi.Book)
                 .Where(o => o.UserId == userId)
                 .OrderByDescending(o => o.OrderDate)
                 .ToListAsync();
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
        }

        
    }
}
