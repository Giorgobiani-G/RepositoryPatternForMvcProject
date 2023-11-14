using Microsoft.EntityFrameworkCore;
using MvcRepository;

namespace Syllogia.Common.EntityFrameworkCore.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> ApplyIncludes<TEntity>(this IQueryable<TEntity> source, string[]? relatedProperties, int relatedPropertiesMaxDepth = 3)
            where TEntity : class
        {
            if (relatedProperties == null || relatedProperties.Length == 0)
                return source;

            var type = typeof(TEntity);
            var entityRelatedProps = LoadableRelatedPropertyAttribute.GetRelatedProperties(type, relatedPropertiesMaxDepth);
            if (entityRelatedProps == null || entityRelatedProps.Count == 0)
                return source;

            var props = relatedProperties.Contains(LoadRelatedProperties.All[0])
                ? entityRelatedProps
                : entityRelatedProps.Intersect(relatedProperties);

            var finalList = FlattenRelatedProperties(props);
            foreach(var prop in finalList)
            {
                source = source.Include(prop);
            }
            return source;
        }

        private static IEnumerable<string> FlattenRelatedProperties(IEnumerable<string> props)
        {
            var finalList = new List<string>();
            foreach(var prop in props.OrderByDescending(x => x))
            {
                if (!finalList.Any(x => x.StartsWith($"{prop}.")))
                    finalList.Add(prop);
            }
            return finalList;
        }
    }
}
