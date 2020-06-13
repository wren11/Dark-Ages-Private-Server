#region

using System.Text;

#endregion

namespace Darkages.Common
{
    public static class Extensions
    {
        private static readonly Encoding encoding = Encoding.GetEncoding(949);

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
    }
}