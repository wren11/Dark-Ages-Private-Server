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
using System.Collections.ObjectModel;
using System.Text;

namespace Darkages.Common
{
    public static class Generator
    {
        public static Random Random;

        public static volatile int SERIAL;

        static Generator()
        {
            Random = new Random();
            GeneratedNumbers = new Collection<int>();
            GeneratedStrings = new Collection<string>();
        }

        public static Collection<int> GeneratedNumbers;
        public static Collection<string> GeneratedStrings;

        public static T RandomEnumValue<T>()
        {
            lock (Random)
            {
                var v = Enum.GetValues(typeof(T));
                return (T)v.GetValue(Random.Next(1, v.Length));
            }
        }

        public static int GenerateNumber()
        {
            var id = (0);

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
