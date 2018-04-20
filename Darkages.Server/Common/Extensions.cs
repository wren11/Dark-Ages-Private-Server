using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Darkages.Common
{
    public static class Extensions
    {
        private readonly static Random rng = new Random();


        public static IEnumerable<int> ArmorRange(this IEnumerable<int> selector, int start, int stop)
        {
            for (int i = start; i < stop; i++)
                yield return i;
        }


        public static T RandomElement<T>(this IList<T> list)
        {
            return list[rng.Next(list.Count)];
        }

        public static T RandomElement<T>(this T[] array)
        {
            return array[rng.Next(array.Length)];
        }


        private static readonly DateTime dateTime = new DateTime(1970, 1, 1).ToLocalTime();
        private static readonly Encoding encoding = Encoding.GetEncoding(949);

        public static byte[] ToByteArray(this string str)
        {
            return encoding.GetBytes(str);
        }

        public static bool IsWithin(this int value, int minimum, int maximum)
        {
            return value >= minimum && value <= maximum;
        }

        public static float Sqrt(float z)
        {
            if (ServerContext.Config.UseFastSqrtMethod)
            {
                if (z == 0)
                    return 0;

                FloatIntUnion u;
                u.tmp = 0;
                var xhalf = 0.5f * z;
                u.f = z;
                u.tmp = 0x5f375a86 - (u.tmp >> 1);
                u.f = u.f * (1.5f - xhalf * u.f * u.f);
                return u.f * z;
            }

            return (float)Math.Sqrt(z);
        }

        public static int Clamp(this int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;

            return value;
        }

        public static int ToUnixTime(this DateTime time)
        {
            return (int)(time - dateTime).TotalSeconds;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatIntUnion
        {
            [FieldOffset(0)] public float f;

            [FieldOffset(0)] public int tmp;
        }


        public class DisposableStopwatch : IDisposable
        {
            private readonly Action<TimeSpan> f;
            private readonly Stopwatch sw;

            public DisposableStopwatch(Action<TimeSpan> f)
            {
                this.f = f;
                sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                sw.Stop();
                f(sw.Elapsed);
            }
        }
    }
}