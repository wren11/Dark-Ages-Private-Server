#region

using System;
using System.Collections.ObjectModel;
using System.Text;

#endregion

namespace Darkages.Common
{
    public static class Generator
    {
        public static Random Random;

        public static volatile int SERIAL;

        public static Collection<int> GeneratedNumbers;
        public static Collection<string> GeneratedStrings;

        static Generator()
        {
            Random = new Random();
            GeneratedNumbers = new Collection<int>();
            GeneratedStrings = new Collection<string>();
        }

        public static T RandomEnumValue<T>()
        {
            lock (Random)
            {
                var v = Enum.GetValues(typeof(T));
                return (T) v.GetValue(Random.Next(1, v.Length));
            }
        }

        public static int GenerateNumber()
        {
            var id = 0;

            do
            {
                lock (Random)
                {
                    id = Random.Next();
                }
            } while (GeneratedNumbers.Contains(id));

            return id;
        }

        public static string CreateString(int size)
        {
            var value = new StringBuilder();

            for (var i = 0; i < size; i++)
            {
                var binary = Random.Next(0, 2);

                switch (binary)
                {
                    case 0:
                        value.Append(Convert.ToChar(Random.Next(65, 91)));
                        break;

                    case 1:
                        value.Append(Random.Next(1, 10));
                        break;
                }
            }

            return value.ToString();
        }

        public static string GenerateString(int size)
        {
            string s;

            do
            {
                s = CreateString(size);
            } while (GeneratedStrings.Contains(s));

            GeneratedStrings.Add(s);

            return s;
        }
    }
}