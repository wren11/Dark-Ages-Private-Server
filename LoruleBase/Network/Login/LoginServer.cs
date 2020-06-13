#region

using System;
using System.Linq;
using System.Net;
using System.Text;
using Darkages.Network.ClientFormats;
using Darkages.Network.ServerFormats;
using Darkages.Storage;
using Darkages.Types;
using Newtonsoft.Json;

#endregion

namespace Darkages.Network.Login
{
    public class LoginServer : NetworkServer<LoginClient>
    {
        public LoginServer(int capacity)
            : base(capacity)
        {
            MServerTable = MServerTable.FromFile("MServerTable.xml");
            Notification = Notification.FromFile("notification.txt");
        }

        public static MServerTable MServerTable { get; set; }
        public static Notification Notification { get; set; }


        protected virtual void Format00Handler(LoginClient client, ClientFormat00 format)
        {
            if (ServerContextBase.Config.UseLobby)
                if (format.Version == ServerContextBase.Config.ClientVersion)
                    client.Send(new ServerFormat00
                    {
                        Type = 0x00,
                        Hash = MServerTable.Hash,
                        Parameters = client.Encryption.Parameters
                    });

            if (ServerContextBase.Config.DevMode)
            {
                var aisling = StorageManager.AislingBucket.Load(ServerContextBase.Config.GameMaster);

                if (aisling != null)
                    LoginAsAisling(client, aisling);
            }
        }

        protected override void Format02Handler(LoginClient client, ClientFormat02 format)
        {
            client.CreateInfo = format;

            var aisling = StorageManager.AislingBucket.Load(format.AislingUsername);

            if (aisling == null)
            {
                client.SendMessageBox(0x00, "\0");
            }
            else
            {
                client.SendMessageBox(0x03, "Character Already Exists.\0");
                client.CreateInfo = null;
            }
        }

        protected override void Format04Handler(LoginClient client, ClientFormat04 format)
        {
            if (client.CreateInfo == null)
            {
                ClientDisconnected(client);
                return;
            }

            var template = Aisling.Create();
            template.Display = (BodySprite) (format.Gender * 16);
            template.Username = client.CreateInfo.AislingUsername;
            template.Password = client.CreateInfo.AislingPassword;
            template.Gender = (Gender) format.Gender;
            template.HairColor = format.HairColor;
            template.HairStyle = format.HairStyle;

            StorageManager.AislingBucket.Save(template);
            client.SendMessageBox(0x00, "\0");
        }

        protected override void Format03Handler(LoginClient client, ClientFormat03 format)
        {
            Aisling aisling = null;

            try
            {
                aisling = StorageManager.AislingBucket.Load(format.Username);

                if (aisling != null)
                {
                    if (aisling.Password != format.Password)
                    {
                        client.SendMessageBox(0x02, "Sorry, Incorrect Password.");
                        return;
                    }
                }
                else
                {
                    client.SendMessageBox(0x02,
                        $"{format.Username} does not exist in this world. You can make this hero by clicking on 'Create'.");
                    return;
                }
            }
            catch (Exception e)
            {
                ServerContext.Error(e);
                client.SendMessageBox(0x02,
                    $"{format.Username} is not supported by the new server. Please remake your character. This will not happen when the server goes to beta.");

                return;
            }

            if (!ServerContextBase.Config.MultiUserLogin)
            {
                var aislings = ServerContextBase.Game.Clients.Where(i =>
                    i?.Aisling != null && i.Aisling.LoggedIn &&
                    i.Aisling.Username.ToLower() == format.Username.ToLower());

                foreach (var obj in aislings)
                    obj.Server.ClientDisconnected(obj);
            }

            LoginAsAisling(client, aisling);
        }

        public void LoginAsAisling(LoginClient client, Aisling aisling)
        {
            if (aisling != null)
            {
                var redirect = new Redirect
                {
                    Serial = Convert.ToString(client.Serial),
                    Salt = Encoding.UTF8.GetString(client.Encryption.Parameters.Salt),
                    Seed = Convert.ToString(client.Encryption.Parameters.Seed),
                    Name = JsonConvert.SerializeObject(new {player = aisling.Username, developer = "wren"})
                };

                ServerContextBase.Redirects.Add(aisling.Username.ToLower());

                if (aisling.Username.Equals(ServerContextBase.Config.GameMaster,
                    StringComparison.CurrentCultureIgnoreCase)) aisling.GameMaster = true;

                client.SendMessageBox(0x00, "\0");
                client.Send(new ServerFormat03
                {
                    EndPoint = new IPEndPoint(Address, ServerContextBase.Config.SERVER_PORT),
                    Redirect = redirect
                });
            }
        }

        protected override void Format0BHandler(LoginClient client, ClientFormat0B format)
        {
            RemoveClient(client);
        }

        protected override void Format10Handler(LoginClient client, ClientFormat10 format)
        {
            client.Encryption.Parameters = format.Parameters;
            client.Send(new ServerFormat60
            {
                Type = 0x00,
                Hash = Notification.Hash
            });
        }

        protected override void Format26Handler(LoginClient client, ClientFormat26 format)
        {
            var aisling = StorageManager.AislingBucket.Load(format.Username);

            if (aisling == null)
            {
                client.SendMessageBox(0x02, "Incorrect Information provided.");
                return;
            }

            if (aisling.Password != format.Password)
            {
                client.SendMessageBox(0x02, "Incorrect Information provided.");
                return;
            }

            if (string.IsNullOrEmpty(format.NewPassword) || format.NewPassword.Length < 3)
            {
                client.SendMessageBox(0x02, "new password not accepted.");
                return;
            }

            aisling.Password = format.NewPassword;
            StorageManager.AislingBucket.Save(aisling);

            client.SendMessageBox(0x00, "\0");
        }

        protected override void Format4BHandler(LoginClient client, ClientFormat4B format)
        {
            client.Send(new ServerFormat60
            {
                Type = 0x01,
                Size = Notification.Size,
                Data = Notification.Data
            });
        }

        protected override void Format57Handler(LoginClient client, ClientFormat57 format)
        {
            if (format.Type == 0x00)
            {
                var redirect = new Redirect
                {
                    Serial = Convert.ToString(client.Serial),
                    Salt = Encoding.UTF8.GetString(client.Encryption.Parameters.Salt),
                    Seed = Convert.ToString(client.Encryption.Parameters.Seed),
                    Name = "socket[" + client.Serial + "]"
                };

                client.Send(new ServerFormat03
                {
                    EndPoint = new IPEndPoint(MServerTable.Servers[0].Address, MServerTable.Servers[0].Port),
                    Redirect = redirect
                });
            }
            else
            {
                client.Send(new ServerFormat56
                {
                    Size = MServerTable.Size,
                    Data = MServerTable.Data
                });
            }
        }

        protected override void Format68Handler(LoginClient client, ClientFormat68 format)
        {
            client.Send(new ServerFormat66());
        }

        protected override void Format7BHandler(LoginClient client, ClientFormat7B format)
        {
            if (format.Type == 0x00)
            {
                Console.WriteLine("Client Requested Metafile: {0}", format.Name);

                client.Send(new ServerFormat6F
                {
                    Type = 0x00,
                    Name = format.Name
                });
            }

            if (format.Type == 0x01)
                client.Send(new ServerFormat6F
                {
                    Type = 0x01
                });
        }

        public override void ClientConnected(LoginClient client)
        {
            client.Send(new ServerFormat7E());
        }
    }
}