using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using System;

namespace Darkages.Scripting.Scripts
{

    [Script("Lorule Arena", "Dean")]
    public class Arena : MapScript
    {
        public GameServerTimer Timer { get; set; }
        public static Random Rand = new Random();

        public Arena(Area area)
            : base(area)
        {
            this.Timer = new GameServerTimer(TimeSpan.FromMilliseconds(2000));
        }

        public override void OnClick(GameClient client, int x, int y)
        {
            client.Send(new ServerFormat29(216, (ushort)x, (ushort)y));
        }

        public override void OnEnter(GameClient client)
        {

        }

        public override void OnLeave(GameClient client)
        {

        }

        public override void OnStep(GameClient client)
        {
            var position = new Position(client.Aisling.X, client.Aisling.Y);

            foreach (var warps in ServerContext.GlobalWarpTemplateCache[Area.ID])
            {
                if (warps.Location.DistanceFrom(position) <= warps.WarpRadius)
                {
                    client.WarpTo(warps);
                }
            }
        }

        ushort animation => (ushort)214;

        public override void Update(TimeSpan elapsedTime)
        {
            Timer.Update(elapsedTime);

            if (Timer.Elapsed)
            {
                //get Aislings on this map.
                var objects = GetObjects<Aisling>(i => Area.Has<Aisling>(i));
                if (objects.Length > 0)
                {
                    foreach (var obj in objects)
                    {
                        if (obj != null)
                            if (obj.WithinRangeOf(25, 25))
                                obj.Client.Send(new ServerFormat29(animation, (ushort)25, (ushort)25));
                    }
                }

                Timer.Reset();
            }
        }
    }

}
