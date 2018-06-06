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
using Darkages.Common;
using Darkages.Systems.Loot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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
                {
                    lock (Generator.Random)
                    {
                        var luck = Generator.GenerateNumber() % 100;

                        if (luck <= item.Weight)
                        {
                            return objs[randomNumber];
                        }
                    }
                }

            }

            return default(T);
        }
    }
}
