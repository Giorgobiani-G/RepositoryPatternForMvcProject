using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MvcRepository.Repository
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEfCoreDbContext<TDbContext>(this IServiceCollection serviceCollection,
            Action<DbContextOptionsBuilder>? optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifeTime = ServiceLifetime.Scoped,
            Action<RepositoryOptions<TDbContext>>? repositoryOptions = null)
            where TDbContext : DbContext
        {
            var contextType = typeof(TDbContext);
            if (DbContexts.GetContextTypes.Contains(contextType))
                return serviceCollection;

            serviceCollection.AddDbContext<TDbContext>(optionsAction, contextLifetime, optionsLifeTime);
            DbContexts.AddContextType<TDbContext>();

            var repoOpts = new RepositoryOptions<TDbContext>();
            repositoryOptions?.Invoke(repoOpts);
            serviceCollection.AddSingleton(repoOpts);

            AddRepositories(serviceCollection, typeof(TDbContext));
            return serviceCollection;
        }

        private static void AddRepositories(IServiceCollection serviceCollection, Type dbContextType)
        {
            var repoInterfaceType = typeof(IRepository<>);
            var repoImplementationType = typeof(Repository<,>);

            foreach (var entityType in GetGenericRepoTypes(dbContextType))
            {
                var genericRepoInterfaceType = repoInterfaceType.MakeGenericType(entityType);
                if (serviceCollection.Any(x => x.ServiceType == genericRepoInterfaceType))
                    continue;

                var genericRepoImplementationType = repoImplementationType.MakeGenericType(dbContextType, entityType);
                serviceCollection.AddScoped(genericRepoInterfaceType, genericRepoImplementationType);
            }

            //foreach (var entityType in GetQueryRepoTypes(dbContextType))
            //{
            //    var genericRepoInterfaceType = queryRepoInterfaceType.MakeGenericType(entityType);
            //    if (serviceCollection.Any(x => x.ServiceType == genericRepoInterfaceType))
            //        continue;

            //    var genericRepoImplementationType = queryRepoImplementationType.MakeGenericType(dbContextType, entityType);
            //    serviceCollection.AddScoped(genericRepoInterfaceType, genericRepoImplementationType);
            //}
        }

        private static IEnumerable<Type> GetGenericRepoTypes(Type dbContextType)
        {
            return dbContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.PropertyType.IsAssignableToGenericType(typeof(DbSet<>))
                && x.GetCustomAttributes<RepositoryAttribute>().FirstOrDefault(y => !y.CreateQueryRepository) == null)
                .Select(x => x.PropertyType.GenericTypeArguments[0]);
        }
    }
}
