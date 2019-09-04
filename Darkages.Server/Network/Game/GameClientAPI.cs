using Darkages.Network.ServerFormats;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darkages.Network.Game
{
    public partial class GameClient : NetworkClient<GameClient>, IDisposable
    {
        /// <summary>
        /// Ports the specified i.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// This Chat command ports the user to a location.
        /// Example chat command: 'port -i:100 -x:5 -y:5'
        public void Port(int i, int x = 0, int y = 0)
        {
            TransitionToMap(i, new Position(x, y));

            SystemMessage("Port: Success.");
        }

        /// <summary>
        ///     This chat command spawns a monster.
        /// </summary>
        /// <param name="t">Name of Monster, Case Sensitive.</param>
        /// <param name="x">X Location to Spawn.</param>
        /// <param name="y">Y Location to Spawn.</param>
        /// <param name="c">The c.</param>
        /// <usage>spawnMonster -t:Undead -x:43 -y:16 -c:10</usage>
        public void Spawn(string t, int x, int y, int c)
        {
            var name = t.Replace("-", string.Empty).Trim();

            var obj = ServerContext.GlobalMonsterTemplateCache
                .FirstOrDefault(i => i.Name.Equals(name, StringComparison.CurrentCulture));

            if (obj != null)
            {
                for (var i = 0; i < c; i++)
                {
                    var mon = Monster.Create(obj, Aisling.Map);
                    if (mon != null)
                    {
                        mon.XPos = x;
                        mon.YPos = y;

                        AddObject(mon);
                    }
                }

                SystemMessage("spawnMonster: Success.");
            }
            else
            {
                SystemMessage("spawnMonster: Failed.");
            }
        }

        public void LearnEverything()
        {
            foreach (var skill in ServerContext.GlobalSkillTemplateCache.Values)
            {
                Skill.GiveTo(Aisling, skill.Name, 100);
            }

            foreach (var spell in ServerContext.GlobalSpellTemplateCache.Values)
            {
                Spell.GiveTo(Aisling, spell.Name, 100);
            }

            LoadSkillBook();
            LoadSpellBook();
        }

        public void ForgetSkill(string s)
        {
            var subject = Aisling.SkillBook.Skills.Values
                .FirstOrDefault(i => i != null
                && i.Template != null
                && !string.IsNullOrEmpty(i.Template.Name) && i.Template.Name.ToLower() == s.ToLower());

            if (subject != null)
            {
                Aisling.SkillBook.Remove(subject.Slot);
                {
                    Send(new ServerFormat2D(subject.Slot));
                }
            }
            LoadSkillBook();
        }

        public void ForgetSpell(string s)
        {
            var subject = Aisling.SpellBook.Spells.Values
                .FirstOrDefault(i => i != null
                && i.Template != null
                && !string.IsNullOrEmpty(i.Template.Name)
                && i.Template.Name.ToLower() == s.ToLower());

            if (subject != null)
            {
                Aisling.SpellBook.Remove(subject.Slot);
                {
                    Send(new ServerFormat18(subject.Slot));
                }
            }
            LoadSpellBook();
        }


        public void GiveExp(int a)
        {
            Monster.DistributeExperience(Aisling, a);
        }

        public void Recover() => Revive();


        public void GiveStr(byte v = 1)
        {
            Aisling._Str += v;

            if (Aisling._Str > ServerContext.Config.StatCap)
                Aisling._Str = ServerContext.Config.StatCap;

            SendStats(StatusFlags.All);
        }

        public void GiveInt(byte v = 1)
        {
            Aisling._Int += v;

            if (Aisling._Int > ServerContext.Config.StatCap)
                Aisling._Int = ServerContext.Config.StatCap;

            SendStats(StatusFlags.All);
        }

        public void GiveWis(byte v = 1)
        {
            Aisling._Wis += v;

            if (Aisling._Wis > ServerContext.Config.StatCap)
                Aisling._Wis = ServerContext.Config.StatCap;

            SendStats(StatusFlags.All);
        }

        public void GiveCon(byte v = 1)
        {
            Aisling._Wis += v;

            if (Aisling._Wis > ServerContext.Config.StatCap)
                Aisling._Wis = ServerContext.Config.StatCap;

            SendStats(StatusFlags.All);
        }

        public void GiveDex(byte v = 1)
        {
            Aisling._Dex += v;

            if (Aisling._Dex > ServerContext.Config.StatCap)
                Aisling._Dex = ServerContext.Config.StatCap;

            SendStats(StatusFlags.All);
        }

        public void GiveHp(int v = 1)
        {
            Aisling._MaximumHp += v;

            if (Aisling._MaximumHp > ServerContext.Config.MaxHP)
                Aisling._MaximumHp = ServerContext.Config.MaxHP;

            SendStats(StatusFlags.All);
        }

        public void GiveMp(int v = 1)
        {
            Aisling._MaximumMp += v;

            if (Aisling._MaximumMp > ServerContext.Config.MaxHP)
                Aisling._MaximumMp = ServerContext.Config.MaxHP;

            SendStats(StatusFlags.All);
        }

        public async Task Effect(ushort n, int d = 1000, int r = 1)
        {
            if (r <= 0)
                r = 1;

            for (var i = 0; i < r; i++)
            {
                Aisling.SendAnimation(n, Aisling, Aisling);

                foreach (var obj in Aisling.MonstersNearby())
                {
                    obj.SendAnimation(n, obj.Position);
                }
                await Task.Delay(d);
            }
        }

        public void StressTest()
        {
            Task.Run(async () =>
            {
                for (int n = 0; n < 5000; n++)
                {
                    for (byte i = 0; i < 100; i++)
                    {
                        await Effect(i, 500);
                    }
                }
            });
        }

        public void GiveScar()
        {
            Aisling.LegendBook.AddLegend(new Legend.LegendItem
            {
                Category = "Event",
                Color = (byte)LegendColor.LightOrange,
                Icon = (byte)LegendIcon.Rogue,
                Value = "Scar of Sgrios"
            });
        }

        public void RevivePlayer(string u)
        {
            var user = GetObject<Aisling>(null, i => i.Username.Equals(u, StringComparison.OrdinalIgnoreCase));

            if (user != null && user.LoggedIn)
                user.Client.Revive();
        }

        public void KillPlayer(string u)
        {
            var user = GetObject<Aisling>(null, i => i.Username.Equals(u, StringComparison.OrdinalIgnoreCase));

            if (user != null)
                user.CurrentHp = 0;
        }

        public void OpenBoard(string n)
        {
            if (ServerContext.GlobalBoardCache.ContainsKey(n))
            {
                var boardListObj = ServerContext.GlobalBoardCache[n];

                if (boardListObj != null && boardListObj.Any())
                    Send(new BoardList(boardListObj));
            }
        }

        public void ReloadObjects(bool all = false)
        {
            var objs = GetObjects(null, i => i != null && i.Serial != Aisling.Serial,
                all ? Get.All : Get.Items | Get.Money | Get.Monsters | Get.Mundanes);

            foreach (var obj in objs) obj.Remove();

            ServerContext.LoadAndCacheStorage();
        }
    }
}
