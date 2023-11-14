using Microsoft.EntityFrameworkCore;
using MvcRepository.Exceptions;
using Syllogia.Common.EntityFrameworkCore.Extensions;
using System.Linq.Expressions;

namespace MvcRepository.Repository
{
    public class Repository<TDbContext, TEntity> : IRepository<TEntity>
        where TEntity : class
        where TDbContext : DbContext
    {
        private readonly TDbContext _context;
        private readonly DbSet<TEntity> _table;

        public Repository(TDbContext context)
        {
            _context = context;
            _table = context.Set<TEntity>();
        }

        public async Task<TEntity> GetByIdAsync(int id, string[]? includeProperties = null, CancellationToken cancellationToken = default)
        {
            var predicate = GenerarateExpressions.Predicate<TEntity>(id);

            var entity = await _table.ApplyIncludes(includeProperties).AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

            if (entity == null)
                throw new Exception("entity not found");

            return entity;
        }

        public async Task<(IEnumerable<TEntity> list, Pager pageDetails)> GetListAsync(int pg = 1,
            int pageSize = 20,
            string? search = null,
            string[]? includeProperties = null,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(search))
            {
                bool isDecimal = decimal.TryParse(search, out decimal decimalValue);

                bool isInt = int.TryParse(search, out _);

                bool isDateTime = DateTime.TryParse(search, out var resDateTime);

                if (isDecimal || isInt)
                {
                    Expression<Func<TEntity, bool>> predicate = GenerarateExpressions.Predicate<TEntity>(decimalValue, search);

                    _table.Where(predicate);
                }
                else if (isDateTime)
                {
                    Expression<Func<TEntity, bool>> predicate = GenerarateExpressions.Predicate<TEntity>(resDateTime);

                    _table.Where(predicate);
                }
                else
                {
                    Expression<Func<TEntity, bool>> predicate = GenerarateExpressions.Predicate<TEntity>(search);

                    _table.Where(predicate);
                }
            }

            var items = _table.Count();

            if (pg < 1)
                pg = 1;

            if (pageSize < 1)
                pageSize = 20;

            Pager pager = new(items, pg, pageSize);

            int skip = (pg - 1) * pageSize;

            List<TEntity> result = await _table.ApplyIncludes(includeProperties).Skip(skip).Take(pager.PageSize).ToListAsync(cancellationToken);

            return (list: result, pageDetails: pager);
        }


        public async Task<IEnumerable<TEntity>> GetListAsync(string[] includeProperties, CancellationToken cancellationToken = default)
        {
            return await _table.ApplyIncludes(includeProperties).AsNoTracking().ToListAsync(cancellationToken: cancellationToken);
        }

        public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _table.AddAsync(entity, cancellationToken);
            await SaveAsync(cancellationToken);
        }

        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _table.Update(entity);
            await SaveAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var typeToDeleted = await GetByIdAsync(id, cancellationToken: cancellationToken) ?? throw new EntityNotFoundException();

            _table.Remove(typeToDeleted);

            await SaveAsync(cancellationToken);
        }

        public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _table.Remove(entity);
            await SaveAsync(cancellationToken);
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> CustomExists(int id)
        {
            var entities = _table.AsAsyncEnumerable();

            await foreach (var entity in entities)
            {
                int pkValue = (int)(entity.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(entity)).Values.ElementAt(0) ?? 0);

                if (pkValue == id)

                    return true;
            }

            return false;
        }

        public IEnumerable<TEntity> GetList()
        {
            return _table.ToList();
        }
    }
}
