///************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
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
using System.Linq;
using Darkages.Network.Game;

namespace Darkages.Types
{
    public class Position
    {
        public ushort X, Y;

        public Position(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }

        public Position(int x, int y) : this((ushort) x, (ushort) y)
        {
        }

        public Position() : this(0, 0)
        {
        }

        public int DistanceFrom(ushort X, ushort Y)
        {
            double XDiff = Math.Abs(X - this.X);
            double YDiff = Math.Abs(Y - this.Y);

            return (int) (XDiff > YDiff ? XDiff : YDiff);
        }

        public int DistanceFrom(Position pos)
        {
            return DistanceFrom(pos.X, pos.Y);
        }


        public bool IsNearby(Position pos)
        {
            return pos.DistanceFrom(X, Y) <= ServerContextBase.GlobalConfig.VeryNearByProximity;
        }

        public static Position operator +(Position a, Direction b)
        {
            var location = new Position(a.X, a.Y);
            switch (b)
            {
                case Direction.North:
                    location.Y--;
                    return location;
                case Direction.East:
                    location.X++;
                    return location;
                case Direction.South:
                    location.Y++;
                    return location;
                case Direction.West:
                    location.X--;
                    return location;
            }

            return location;
        }

        public static Direction operator -(Position a, Position b)
        {
            if (a.X == b.X && a.Y == b.Y + 1)
                return Direction.North;
            if (a.X == b.X && a.Y == b.Y - 1)
                return Direction.South;
            if (a.X == b.X + 1 && a.Y == b.Y)
                return Direction.West;
            if (a.X == b.X - 1 && a.Y == b.Y)
                return Direction.East;

            return Direction.UnDefined;
        }

        public TileContentPosition[] SurroundingContent(Area map)
        {
            var list = new List<TileContentPosition>();

            if (X > 0)
            {
                list.Add(new TileContentPosition(
                    new Position(X - 1, Y),
                    map.ObjectGrid[X - 1, Y].Sprites.Count == 0 ? TileContent.Wall : TileContent.None));

            }

            if (Y > 0)
            {
                list.Add(new TileContentPosition(
                    new Position(X, Y - 1),
                    map.ObjectGrid[X, Y - 1].Sprites.Count == 0 ? TileContent.Wall : TileContent.None));

            }

            if (X < map.Rows - 1)
            {
                list.Add(new TileContentPosition(
                    new Position(X + 1, Y),
                    map.ObjectGrid[X + 1, Y].Sprites.Count == 0 ? TileContent.Wall : TileContent.None));

            }

            if (Y < map.Cols - 1)
            {
                list.Add(new TileContentPosition(
                    new Position(X, Y + 1),
                    map.ObjectGrid[X, Y + 1].Sprites.Count == 0 ? TileContent.Wall : TileContent.None));

            }


            return list.ToArray();
        }


        public bool IsNextTo(Position pos, int distance = 1)
        {
            if (X == pos.X && Y + distance == pos.Y) return true;
            if (X == pos.X && Y - distance == pos.Y) return true;
            if (X == pos.X + distance && Y == pos.Y) return true;
            if (X == pos.X - distance && Y == pos.Y) return true;

            return false;
        }

        public class TileContentPosition
        {
            public TileContentPosition(Position pos, TileContent content)
            {
                Position = pos;
                Content = content;
            }

            public Position Position { get; set; }
            public TileContent Content { get; set; }
        }
    }
}