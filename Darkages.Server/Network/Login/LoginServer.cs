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
using Darkages.Network.ClientFormats;
using Darkages.Network.ServerFormats;
using Darkages.Security;
using Darkages.Storage;
using Darkages.Types;
using System;
using System.Linq;
using System.Net;

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

        public static int BotIncrement = 1;

        public Aisling CreateBot()
        {

            var gender = 1;

            //create aisling from default template.
            var template = Aisling.Create();
            unchecked
            {
                template.Display = (BodySprite)(gender * 16);
            }
            template.Username = "bot_" + BotIncrement;
            template.Password = "bot";
            template.Gender = Gender.Male;
            template.HairColor = (byte)Common.Generator.Random.Next(1, 6);
            template.HairStyle = (byte)Common.Generator.Random.Next(1, 8);

            var items = new ItemTemplate[] {
                ServerContext.GlobalItemTemplateCache["Loures Signet Ring"],
                ServerContext.GlobalItemTemplateCache["Black Stone Ring"],
                ServerContext.GlobalItemTemplateCache["Hy-Brasyl Battle Axe"],
                ServerContext.GlobalItemTemplateCache["Hy-Brasyl Plated Helmet"],
                ServerContext.GlobalItemTemplateCache["Leather Gauntlet"],
                ServerContext.GlobalItemTemplateCache["Leather Gauntlet"],

                ServerContext.GlobalItemTemplateCache["Leather Greaves"],
                ServerContext.GlobalItemTemplateCache["Gold Earrings"],
                ServerContext.GlobalItemTemplateCache["Dark Bone Necklace"],
                ServerContext.GlobalItemTemplateCache["Dark Belt"],

                ServerContext.GlobalItemTemplateCache["Hy-Brasyl Plate"],
            };

            template._Str = 200;
            template.ExpLevel = 99;
            template.X = 5;
            template.Y = 5;

            template.Map.Update(template.X, template.Y, template);

            foreach (var item in items)
            {
                var i = Item.Create(template, item);

                if (i.GiveTo(template))
                {

                }
            }

            template.IsBot = true;

            Skill.GiveTo(template, "Assail", 1);

            Console.WriteLine("Bot Created: " + template.Username);

            BotIncrement++;

            return template;

        }

        /// <summary>
        ///     Send Encryption Parameters.
        /// </summary>
        protected override void Format00Handler(LoginClient client, ClientFormat00 format)
        {
            if (format.Version == ServerContext.Config.Version)
                client.Send(new ServerFormat00
                {
                    Type = 0x00,
                    Hash = MServerTable.Hash,
                    Parameters = client.Encryption.Parameters
                });
        }

        /// <summary>
        ///     Login Client - Create New Aisling, Choose Username/password.
        /// </summary>
        protected override void Format02Handler(LoginClient client, ClientFormat02 format)
        {
            //save information to memory.
            client.CreateInfo = format;
            client.SendMessageBox(0x00, "\0");
        }

        /// <summary>
        ///     Login Client - Save Character Template.
        /// </summary>
        protected override void Format04Handler(LoginClient client, ClientFormat04 format)
        {
            //make sure the first step was done first.
            if (client.CreateInfo == null)
            {
                ClientDisconnected(client);
                return;
            }

            //create aisling from default template.
            var template = Aisling.Create();
            template.Display = (BodySprite)(format.Gender * 16);
            template.Username = client.CreateInfo.AislingUsername;
            template.Password = client.CreateInfo.AislingPassword;
            template.Gender = (Gender)format.Gender;
            template.HairColor = format.HairColor;
            template.HairStyle = format.HairStyle;

            Console.WriteLine  ("New character Created: " + template.Username);

            StorageManager.AislingBucket.Save(template);
            client.SendMessageBox(0x00, "\0");
        }

        /// <summary>
        ///     Login - Check username/password. Proceed to Game Server.
        /// </summary>
        protected override void Format03Handler(LoginClient client, ClientFormat03 format)
        {
            Aisling _aisling = null;
            var IsBot = format.Username == "bot";

            if (!IsBot)
            {

                try
                {
                    _aisling = StorageManager.AislingBucket.Load(format.Username);

                    if (_aisling != null)
                    {
                        if (_aisling.Password != format.Password)
                        {
                            client.SendMessageBox(0x02, "Sorry, Incorrect Password.");
                            return;
                        }
                    }
                    else
                    {
                        client.SendMessageBox(0x02, string.Format("{0} does not exist in this world. You can make this hero by clicking on 'Create'.", format.Username));
                        return;
                    }
                }
                catch
                {
                    client.SendMessageBox(0x02, string.Format("{0} is not supported by the new server. Please remake your character. This will not happen when the server goes to beta.", format.Username));
                    return;
                }
            }

            if (IsBot)
            {
                _aisling = CreateBot();
                ServerContext.ConnectedBots[_aisling.Username] = _aisling;
            }

            if (!IsBot)
            {
                var aislings = GetObjects<Aisling>(_aisling.Map, i => i.Username == format.Username && format.Password == i.Password);
                foreach (var aisling in aislings)
                {
                    aisling.Client.SendMessage(0x02, "You have been replaced by someone else.");
                    aisling.Client.Server.ClientDisconnected(aisling.Client);
                }
            }

            if (_aisling != null)
            {

                var redirect = new Redirect
                {
                    Serial = client.Serial,
                    Salt = client.Encryption.Parameters.Salt,
                    Seed = client.Encryption.Parameters.Seed,
                    Name = _aisling.Username
                };

                ServerContext.GlobalRedirects.Add(redirect);

                client.SendMessageBox(0x00, "\0");

                client.Send(new ServerFormat03
                {
                    EndPoint = new IPEndPoint(Address, ServerContext.DefaultPort),
                    Redirect = redirect
                });
            }
        }

        /// <summary>
        ///     Client Closed Connection (Closed Darkages.exe), Remove them.
        /// </summary>
        protected override void Format0BHandler(LoginClient client, ClientFormat0B format)
        {
            RemoveClient(client);
        }

        /// <summary>
        ///     Redirect Client from Lobby Server to Login Server Automatically.
        /// </summary>
        protected override void Format10Handler(LoginClient client, ClientFormat10 format)
        {
            var redirect = ServerContext.GlobalRedirects.FirstOrDefault(o => o.Serial == format.Id);

            if (redirect == null)
            {
                ClientDisconnected(client);
                return;
            }

            if (redirect.Type == 2)
                ServerContext.Game.RemoveClient(redirect.Client);

            client.Encryption.Parameters = new SecurityParameters(redirect.Seed, redirect.Salt);
            client.Send(new ServerFormat60
            {
                Type = 0x00,
                Hash = Notification.Hash
            });
            ServerContext.GlobalRedirects.Remove(redirect);
        }

        /// <summary>
        ///     Login Client - Update Password.
        /// </summary>
        protected override void Format26Handler(LoginClient client, ClientFormat26 format)
        {
            var _aisling = StorageManager.AislingBucket.Load(format.Username);
            if (_aisling == null)
            {
                client.SendMessageBox(0x02, "Incorrect Information provided.");
                return;
            }

            if (_aisling.Password != format.Password)
            {
                client.SendMessageBox(0x02, "Incorrect Information provided.");
                return;
            }

            if (string.IsNullOrEmpty(format.NewPassword) || format.NewPassword.Length < 3)
            {
                client.SendMessageBox(0x02, "new password not accepted.");
                return;
            }

            //Update new password.
            _aisling.Password = format.NewPassword;
            //Update and Store Information.
            StorageManager.AislingBucket.Save(_aisling);

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

        protected override void Format66Handler(LoginClient client, ClientFormat66 format)
        {

        }

        protected override void Format57Handler(LoginClient client, ClientFormat57 format)
        {
            if (format.Type == 0x00)
            {
                var redirect = new Redirect
                {
                    Serial = client.Serial,
                    Salt = client.Encryption.Parameters.Salt,
                    Seed = client.Encryption.Parameters.Seed,
                    Name = "socket[" + client.Serial + "]"
                };

                ServerContext.GlobalRedirects.Add(redirect);

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
                    Data = MServerTable.Data,
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
                client.Send(new ServerFormat6F
                {
                    Type = 0x00,
                    Name = format.Name
                });

            if (format.Type == 0x01)
                client.Send(new ServerFormat6F
                {
                    Type = 0x01
                });
        }

        public override bool AddClient(LoginClient client)
        {
            return base.AddClient(client);
        }

        public override void ClientConnected(LoginClient client)
        {
            client.Send(new ServerFormat7E());
        }
    }
}
