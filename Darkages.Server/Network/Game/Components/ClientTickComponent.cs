using Darkages.Types;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                //UpdateDatabase();
                timer.Reset();
            }
        }

        public class EntityObj
        {
            public Type RefType;
            public string Name;
            public string Data;

            public DateTime Updated;
            public string UserName;

            [BsonId] public int Serial { get; set; }
        }

        private void UpdateDatabase()
        {
            using (var db = new LiteDatabase(ServerContext.StoragePath
                                             + "\\" + ServerContext.Config.DBName))
            {
                var dbSprites = db.GetCollection<EntityObj>("LorTemp");
                var objects = GetObjects(null, sprite => sprite != null, Get.All).ToArray();

                foreach (var obj in objects)
                {
                    var bObj = new EntityObj
                    {
                        Serial = obj.Serial,
                        Data = JsonConvert.SerializeObject(obj),
                        RefType = obj.GetType(),
                        Updated = DateTime.UtcNow,
                        UserName = (obj is Aisling o) ? o.Username : string.Empty,
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

                        if (obj is Aisling aisling)
                        {
                            if (!dbSprites.Exists(i => i.UserName == aisling.Username))
                            {
                                dbSprites.Insert(bObj);
                            }
                        }
                        else
                        {
                            if (!dbSprites.Exists(i => i.Serial == obj.Serial))
                            {
                                dbSprites.Insert(bObj);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }

                }

                dbSprites.EnsureIndex(i => i.Serial);
            }
        }
    }
}