using BookStore.API.Data;
using BookStore.API.Repositories.IRepositories;

namespace BookStore.API.Repositories.RepositoryImpl
{
    public class UnitOfWork:IUnitOfWork
    {
        private readonly BookStoreDbContext _context;
        public IBookRepository Books { get; }
        public ICategoryRepository Categories { get; }
        public ICartRepository Carts { get; }
        public IOrderRepository Orders { get; }
        public UnitOfWork(BookStoreDbContext context,
            IBookRepository bookRepository,
            ICategoryRepository categoryRepository,
            ICartRepository cartRepository,
            IOrderRepository orderRepository)
            
        {
            _context = context;
            Books = bookRepository;
            Categories = categoryRepository;
            Carts= cartRepository;
            Orders= orderRepository;

        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                await action();

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
        }
    }
}
