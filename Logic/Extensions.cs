using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UnicodeMAP.Logic
{
    public static class StringExtension
    {
        public static int NthIndexOf(this string target, string value, int n)
        {
            Match m = Regex.Match(target, "((" + Regex.Escape(value) + ").*?){" + n + "}");

            if (m.Success)
                return m.Groups[2].Captures[n - 1].Index;
            else
                return -1;
        }

    }
    public static class LinqExtension
    {
        //public static IQueryable<TSource> Between<TSource, TKey>(
        //    this IQueryable<TSource> source,
        //    Expression<Func<TSource, TKey>> keySelector,
        //    TKey low, TKey high) where TKey : IComparable<TKey>
        //{
        //    Expression key = Expression.Invoke(keySelector, keySelector.Parameters.ToArray());
        //    Expression lowerBound = Expression.GreaterThanOrEqual(key, Expression.Constant(low));
        //    Expression upperBound = Expression.LessThanOrEqual(key, Expression.Constant(high));
        //    Expression and = Expression.AndAlso(lowerBound, upperBound);
        //    Expression<Func<TSource, bool>> lambda = Expression.Lambda<Func<TSource, bool>>(and, keySelector.Parameters);
        //    return source.Where(lambda);
        //}
    }
}
