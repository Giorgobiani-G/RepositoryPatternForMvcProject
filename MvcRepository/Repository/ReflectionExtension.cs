using System.Reflection;

namespace MvcRepository.Repository
{
    public static class ReflectionExtension
    {
        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            if (givenType.GetTypeInfo().IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            foreach (Type interfaceType in givenType.GetInterfaces())
            {
                if (interfaceType.GetTypeInfo().IsGenericType && interfaceType.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
            }

            if (givenType.GetTypeInfo().BaseType == null)
            {
                return false;
            }
            return givenType.GetTypeInfo().BaseType.IsAssignableToGenericType(genericType);
        }
    }
}
