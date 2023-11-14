using MvcRepository.Repository;
using System.Collections.Concurrent;
using System.Reflection;

namespace MvcRepository
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LoadableRelatedPropertyAttribute : Attribute
    {
        public bool IgnoreCircularReferenceCheck { get; private set; }

        private static readonly ConcurrentDictionary<Type, List<string>> RelatedProperties = new();
        public LoadableRelatedPropertyAttribute(bool ignoreCircularReferenceCheck = false)
        {
            IgnoreCircularReferenceCheck = ignoreCircularReferenceCheck;
        }

        public static List<string> GetRelatedProperties(Type type, int maxDepth = 3)
        {
            if (!RelatedProperties.TryGetValue(type, out var relatedProperties))
            {
                relatedProperties = new List<string>();
                FillRelatedProperties(type, null, false, relatedProperties, null, maxDepth);
                RelatedProperties.AddOrUpdate(type, relatedProperties, (t, l) => relatedProperties);
            }
            return relatedProperties;
        }

        private static void FillRelatedProperties(Type type, Type? parentType, bool ignoreCircularReferenceCheck,
            List<string> relatedProperties, string? prefix, int maxDepth, int currentDepth = 0)
        {
            if (currentDepth > maxDepth)
                return;

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => CustomAttributeExtensions.GetCustomAttributes<LoadableRelatedPropertyAttribute>((MemberInfo)x).Any())
                .ToList();

            if (props.Count > 0)
            {
                foreach (var prop in props)
                {
                    var attribute = prop.GetCustomAttributes<LoadableRelatedPropertyAttribute>().FirstOrDefault();
                    if (attribute == null)
                        continue;

                    var propPath = prefix == null ? prop.Name : $"{prefix}.{prop.Name}";

                    var propType = prop.PropertyType;

                    if (propType.IsAssignableToGenericType(typeof(IEnumerable<>)))
                        propType = propType.GetGenericArguments()[0];

                    //Check circular references
                    if (!attribute.IgnoreCircularReferenceCheck && !ignoreCircularReferenceCheck && propType == parentType)
                        continue;

                    relatedProperties.Add(propPath);
                    FillRelatedProperties(propType, type, attribute.IgnoreCircularReferenceCheck, relatedProperties, propPath, maxDepth, currentDepth + 1);
                }
            }
        }
    }
}
