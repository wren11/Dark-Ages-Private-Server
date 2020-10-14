#region

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Darkages.Common
{
    public static class Extensions
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding(949);

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
                if (seenKeys.Add(keySelector(element)))
                    yield return element;
        }

        public static int Clamp(this int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;

            return value;
        }

        public static bool IsWithin(this int value, int minimum, int maximum)
        {
            return value >= minimum && value <= maximum;
        }

        public static byte[] ToByteArray(this string str)
        {
            return Encoding.GetBytes(str);
        }
    }
}