#region

using System.Threading.Tasks;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Destroyer", "Dean")]
    public class Destroyer : SkillScript
    {
        public Skill _skill;
        public Sprite Target;

        public Destroyer(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public void DestroyAll(GameClient client)
        {
            new TaskFactory().StartNew(() =>
            {
                var objects = GetObjects(client.Aisling.Map, i => i.WithinRangeOf(client.Aisling), Get.Monsters);

                var action = new ServerFormat1A
                {
                    Serial = client.Aisling.Serial,
                    Number = 0x02,
                    Speed = 40
                };

                client.Aisling.Show(Scope.NearbyAislings, action);

                foreach (var obj in objects)
                {
                    (obj as Monster).Target = client.Aisling;
                    (obj as Monster).GenerateRewards(client.Aisling);

                    client.SendAnimation(301, obj, client.Aisling);
                    obj.ApplyDamage(client.Aisling, 999999999, 40);
                }
            });
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (Target != null)
                    client.Aisling.Show(Scope.NearbyAislings,
                        new ServerFormat29(Skill.Template.MissAnimation, (ushort) Target.XPos, (ushort) Target.YPos));
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (client.Aisling != null)
                    DestroyAll(client);
            }
        }
    }
}