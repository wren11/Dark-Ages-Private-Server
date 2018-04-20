///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
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
