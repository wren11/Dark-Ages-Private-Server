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
using Darkages.Types;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Darkages
{
    public partial class Area
    {
        public Sprite Owner { get; set; }

        public class MapTile
        {
            public ushort X, Y;

            public TileContent BaseObject { get; set; }

            public bool HasWall => BaseObject == TileContent.Wall;
          
            private ConcurrentDictionary<int, Sprite> Objects = new ConcurrentDictionary<int, Sprite>();

            public List<Sprite> Sprites
            {
                get => Objects.Select(i => i.Value).ToList(); 
            }

            public void Empty()
            {
                Objects = new ConcurrentDictionary<int, Sprite>();
            }

            public bool SpotVacant()
            {
                var result = true;

                if (BaseObject == TileContent.Warp)
                    return true;

                if (BaseObject == TileContent.Wall)
                    return false;


                for (int i = 0; i < Sprites.Count; i++)
                {
                    if (Sprites[i] is Monster)
                    {
                        if ((Sprites[i] as Monster).Template.IgnoreCollision)
                        {
                            return false;
                        }
                    }

                    if (Sprites[i] is Mundane)
                    {
                        return false;
                    }

                    if (Sprites[i].CurrentHp > 0)
                    {
                        return false;
                    }
                }

                return result;
            }

            public bool Add(Sprite obj)
            {
                return Objects.TryAdd(obj.Serial, obj);
            }

            public bool Remove(Sprite obj)
            {
                Sprite removedObj;
                return Objects.TryRemove(obj.Serial, out removedObj);
            }
        }
    }
}
