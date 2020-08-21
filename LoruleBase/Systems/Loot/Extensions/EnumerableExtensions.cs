#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Common;
using Darkages.Systems.Loot.Interfaces;

#endregion

namespace Darkages.Systems.Loot.Extensions
{
    public static class EnumerableExtensions
    {
        private static readonly Random Random = new Random();

        public static T WeightedChoice<T>(this IEnumerable<T> items, double sum) where T : IWeighable
        {
            lock (Random)
            {
                var randomNumber = Random.Next(0, items.Count());
                var objs = items.ToArray();

                foreach (var item in items)
                    lock (Generator.Random)
                    {
                        var luck = (short)Math.Abs(NextFloat(Generator.Random));

                        if (luck < 0 || luck > 0) return objs[randomNumber];
                    }
            }

            return default;
        }

        private static float NextFloat(Random random)
        {
            var mantissa = random.NextDouble() * 2.0 - 1.0;
            var exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }
    }
}