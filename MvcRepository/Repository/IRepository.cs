namespace MvcRepository.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetListAsync(string[] includeProperties = null, CancellationToken cancellationToken = default);
        Task<(IEnumerable<TEntity> list, Pager pageDetails)> GetListAsync(int pg, int pageSize, string search = null, string[] includeProperties = null, CancellationToken cancellationToken = default);
        IEnumerable<TEntity> GetList();
        Task<TEntity> GetByIdAsync(int id, string[] includeProperties = null, CancellationToken cancellationToken = default);
        Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task SaveAsync(CancellationToken cancellationToken = default);
        Task<bool> CustomExists(int id);
    }
}
