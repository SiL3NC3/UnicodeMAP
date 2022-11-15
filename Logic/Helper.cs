using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Unicode;

namespace UnicodeMAP.Logic
{
    static class Helper
    {
        // String Helper
        public static string IntToHex(int number)
        {
            return number.ToString("X");
        }
        public static int HexToInt(string hex)
        {
            return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }
        public static string GetUnicodeDisplayText(string hex)
        {
            return UnicodeInfo.GetDisplayText(HexToInt(hex));
        }

        public static string[] Split(this string str, string splitter)
        {
            return str.Split(new[] { splitter }, StringSplitOptions.None);
        }
        public static int CountLines(string str)
        {
            return str.Split(new string[] { "\n" }, StringSplitOptions.None).Count();
        }
        public static string GetBetween(string content, string startString, string endString)
        {
            int Start = 0, End = 0;
            if (content.Contains(startString) && content.Contains(endString))
            {
                Start = content.IndexOf(startString, 0) + startString.Length;
                End = content.IndexOf(endString, Start);
                return content.Substring(Start, End - Start);
            }
            else
                return string.Empty;
        }

        // XML Helper
        public static void Serialize<T>(this T baseType, string filePath)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                serializer.Serialize(stream, baseType);
            }
        }
        public static T Deserialize<T>(string filename)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

            if (!File.Exists(filename))
            {
                string ShortDirectory = (filename.Length > 30) ? (filename.Substring(0, 30) + "...") : (filename);
                throw new FileNotFoundException(string.Format("File \"{0}\" not found", ShortDirectory));
            }

            T theclass;

            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                theclass = (T)serializer.Deserialize(stream);
            }

            return theclass;
        }

        // HttpClient Helper
        public static async Task<StreamReader> DownloadAsync(string sourceUrl)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(sourceUrl).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return new StreamReader(stream);
            }
        }

        // LINQ Helper
        public static IQueryable<TSource> Between<TSource, TKey, TLowHigh>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector, TLowHigh low, TLowHigh high, bool inclusive = true)
          where TKey : IComparable<TKey>
        {
            var key = Expression.Invoke(keySelector, keySelector.Parameters.ToArray());

            var intLow = int.Parse(low.ToString());
            var intHigh = int.Parse(high.ToString());

            var lowerBound = (inclusive)
                    ? Expression.GreaterThanOrEqual(key, Expression.Constant(intLow, typeof(int)))
                    : Expression.GreaterThan(key, Expression.Constant(intLow, typeof(int)));

            var upperBound = (inclusive)
                    ? Expression.LessThanOrEqual(key, Expression.Constant(intHigh, typeof(int)))
                    : Expression.LessThan(key, Expression.Constant(intHigh, typeof(int)));

            var and = Expression.AndAlso(lowerBound, upperBound);
            var lambda = Expression.Lambda<Func<TSource, bool>>(
                            and, keySelector.Parameters);

            return source.Where(lambda);
        }
        public static IQueryable<TSource> Between<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, TKey low, TKey high) where TKey : IComparable<TKey>
        {
            Expression key = Expression.Invoke(keySelector, keySelector.Parameters.ToArray());
            Expression lowerBound = Expression.GreaterThanOrEqual(key, Expression.Constant(low));
            Expression upperBound = Expression.LessThanOrEqual(key, Expression.Constant(high));
            Expression and = Expression.AndAlso(lowerBound, upperBound);
            Expression<Func<TSource, bool>> lambda = Expression.Lambda<Func<TSource, bool>>(and, keySelector.Parameters);
            return source.Where(lambda);
        }
    }
}
