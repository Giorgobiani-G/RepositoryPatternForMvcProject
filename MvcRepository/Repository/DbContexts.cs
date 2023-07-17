using Microsoft.EntityFrameworkCore;

namespace MvcRepository.Repository
{
    internal class DbContexts
    {
        private static readonly List<Type> ContextTypes = new List<Type>();

        internal static void AddContextType<TContext>()
            where TContext : DbContext
        {
            var contextType = typeof(TContext);
            if (ContextTypes.Contains(contextType))
                return;

            ContextTypes.Add(contextType);
        }

        internal static Type[] GetContextTypes => ContextTypes.ToArray();
    }
}
