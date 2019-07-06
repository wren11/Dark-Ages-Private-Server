using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Darkages.Types;
using LiteDB;
using Newtonsoft.Json;

namespace Darkages.Network.Game.Components
{
    public class ClientTickComponent : GameServerComponent
    {
        private readonly GameServerTimer timer;

        public ClientTickComponent(GameServer server)
            : base(server)
        {
            timer = new GameServerTimer(
                TimeSpan.FromSeconds(30));
        }

        public override void Update(TimeSpan elapsedTime)
        {
            timer.Update(elapsedTime);

            if (timer.Elapsed)
            {
                UpdateDatabase();
                timer.Reset();
            }
        }

        public class EntityObj
        {
            public Type RefType;
            public string Name;
            public string Data;

            public DateTime Updated;

            [BsonId] public int Serial { get; set; }
        }

        private void UpdateDatabase()
        {
            Task.Run(() =>
            {
                using (var db = new LiteDatabase(ServerContext.StoragePath 
                                                 + "\\" + ServerContext.Config.DBName))
                {
                    db.DropCollection("ObjectCollection");

                    var dbSprites = db.GetCollection<EntityObj>("ObjectCollection");
                    var objects = GetObjects(null, sprite => sprite != null, Get.All)
                        .ToArray();



                    foreach (var obj in objects)
                    {

                        var bObj = new EntityObj
                        {
                            Serial = obj.Serial,
                            Data = JsonConvert.SerializeObject(obj),
                            RefType = obj.GetType(),
                            Updated = DateTime.UtcNow
                        };

                        if (obj is Monster)
                            bObj.Name = "Monster";
                        if (obj is Aisling)
                            bObj.Name = "Aisling";
                        if (obj is Item)
                            bObj.Name = "Item";
                        if (obj is Mundane)
                            bObj.Name = "Mundane";

                        try
                        {
                            dbSprites.Insert(bObj);

                        }
                        catch
                        {
                        }

                    }

                    dbSprites.EnsureIndex(i => i.Serial);
                }
            });
        }
    }
}