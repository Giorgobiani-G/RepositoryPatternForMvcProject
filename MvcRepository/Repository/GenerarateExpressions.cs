using System.Linq.Expressions;
using System.Reflection;

namespace MvcRepository.Repository
{
    public class GenerarateExpressions
    {
        public static Expression<Func<TEntity, bool>> Predicate<TEntity>(int id)
        {
            Type type = typeof(TEntity);

            string pkName = type.GetProperties().ElementAt(0).Name;

            ParameterExpression parameter = Expression.Parameter(type, "x");
            MemberExpression prop = Expression.Property(parameter, pkName);
            ConstantExpression contant = Expression.Constant(id);
            BinaryExpression body = Expression.Equal(prop, contant);

            return Expression.Lambda<Func<TEntity, bool>>(body, new[] { parameter });
        }

        public static Expression<Func<TEntity, bool>> Predicate<TEntity>(decimal decimalValue, string? search = null)
        {
            var type = typeof(TEntity);

            PropertyInfo[] properties;

            ParameterExpression prm = Expression.Parameter(type, "x");

            ConstantExpression searchValue = Expression.Constant(search);

            ConstantExpression searchDecimalValue = Expression.Constant(decimalValue);

            UnaryExpression converted = Expression.Convert(searchDecimalValue, typeof(object));

            IEnumerable<PropertyInfo> wholeNumberProps = type.GetProperties().Where(x => x.PropertyType == typeof(byte) || x.PropertyType == typeof(byte?) ||
                                                               x.PropertyType == typeof(short) || x.PropertyType == typeof(short?) ||
                                                               x.PropertyType == typeof(int) || x.PropertyType == typeof(int?)).ToArray();

            MethodInfo? toStringMethod = typeof(object).GetMethod(nameof(ToString));

            MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            IEnumerable<MemberExpression> wholeNumberMemberExpressions = wholeNumberProps.Select(prp => Expression.Property(prm, prp));

            IEnumerable<MethodCallExpression> wholeNumbersToString = wholeNumberMemberExpressions.Select(mem => Expression.Call(mem, toStringMethod!));

            IEnumerable<Expression> wholeNumberExpressions = wholeNumbersToString.Select(expr => Expression.Call(expr, containsMethod!, searchValue));

            properties = type.GetProperties().Where(x => x.PropertyType == typeof(decimal) || x.PropertyType == typeof(decimal?)).ToArray();

            IEnumerable<MemberExpression> memberExpressions = properties.Select(prp => Expression.Property(prm, prp));

            MethodInfo? equalsMethod = typeof(object).GetMethod("Equals", new[] { typeof(object) });

            IEnumerable<Expression> expressions = memberExpressions.Select(mem => Expression.Call(mem, equalsMethod!, converted));

            IEnumerable<Expression> combined = expressions.Concat(wholeNumberExpressions);

            Expression body = combined.Aggregate((prev, current) => Expression.Or(prev, current));

            Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(body, prm);

            return lambda;
        }

        public static Expression<Func<TEntity, bool>> Predicate<TEntity>(DateTime resDateTime)
        {
            var type = typeof(TEntity);

            PropertyInfo[] properties;

            ParameterExpression prm = Expression.Parameter(type, "x");

            ConstantExpression searchValueDateTime = Expression.Constant(resDateTime);

            Expression converted = Expression.Convert(searchValueDateTime, typeof(object));

            properties = type.GetProperties().Where(x => x.PropertyType == typeof(DateTime) || x.PropertyType == typeof(DateTime?)).ToArray();

            IEnumerable<MemberExpression> memberExpressions = properties.Select(prp => Expression.Property(prm, prp));

            MethodInfo? equalsMethod = typeof(object).GetMethod("Equals", new[] { typeof(object) });

            IEnumerable<Expression> expressions = memberExpressions.Select(mem => Expression.Call(mem, equalsMethod!, converted));

            Expression body = expressions.Aggregate((prev, current) => Expression.Or(prev, current));

            Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(body, prm);

            return lambda;
        }

        public static Expression<Func<TEntity, bool>> Predicate<TEntity>(string search)
        {
            var type = typeof(TEntity);

            ConstantExpression searchValue = Expression.Constant(search);

            PropertyInfo[] properties;

            ParameterExpression prm = Expression.Parameter(type, "x");

            properties = type.GetProperties().Where(x => x.PropertyType == typeof(string)).ToArray();

            MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            IEnumerable<Expression> expressions = properties.Select(prp => Expression.Call(Expression.Property(prm, prp), containsMethod!, searchValue));

            Expression body = expressions.Aggregate((prev, current) => Expression.Or(prev, current));

            Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(body, prm);

            return lambda;
        }
    }
}
