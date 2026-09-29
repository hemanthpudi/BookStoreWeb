namespace BookStore.API.Repositories.IRepositories
{
    public interface IUnitOfWork
    {
        IBookRepository Books { get; }
        ICategoryRepository Categories { get; }
        ICartRepository Carts { get; }
        IOrderRepository Orders { get; }   
        Task SaveChangesAsync();
        Task ExecuteInTransactionAsync(Func<Task> action);

    }
}
