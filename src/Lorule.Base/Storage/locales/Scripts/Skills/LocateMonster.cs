#region

using System.IO;
using System.Text;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Locate Monster", "Test")]
    public class LocateMonster : SkillScript
    {
        public LocateMonster(Skill skill) : base(skill)
        {
        }

        public override void OnFailed(Sprite sprite)
        {
        }

        public override void OnSuccess(Sprite sprite)
        {
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling aisling)
            {
                var objects = aisling.GetObjects(null, i => true, Get.All);
                var sb = new StringBuilder();

                foreach (var obj in objects)
                    sb.AppendLine(string.Format("{0} {1} {2} {3} {4} {5}", obj.Position.X, obj.Position.Y, obj.Map.Name,
                        obj.CurrentMapId,
                        obj.Direction, obj.EntityType));

                File.WriteAllText("objdump.txt", sb.ToString());
            }
        }
    }
}