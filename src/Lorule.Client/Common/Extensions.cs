#region

using System;
using System.Diagnostics;
using System.Text;

#endregion

namespace Darkages.Common
{
    public static class Extensions
    {
        private static readonly Encoding encoding = Encoding.GetEncoding(949);

        public static T Eval<T>(this string code) where T : class
        {
            return default; // return Evaluator.Evaluate(code) as T;
        }

        public static void Run(this string code, bool repQuotes = false)
        {
            var run = repQuotes ? code.Replace("'", "\"") : code;

            //Evaluator.Run(run);
        }

        public static byte[] ToByteArray(this string str)
        {
            return encoding.GetBytes(str);
        }

        public static bool IsWithin(this int value, int minimum, int maximum)
        {
            return value >= minimum && value <= maximum;
        }

        public static int Clamp(this int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;

            return value;
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