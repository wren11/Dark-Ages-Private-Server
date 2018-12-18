using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System;

namespace Darkages.Storage.locales.Scripts.Weapons
{
    [Script("Snow Secret", author: "Dean")]
    public class SnowSecret : WeaponScript
    {
        public SnowSecret(Item item) : base(item)
        {

        }

        public override void OnUse(Sprite sprite, Action<int> cb)
        {

            var enemy = sprite.GetInfront(8);
            var count = 1;
            if (enemy != null)
            {
                foreach (var i in enemy)
                {
                    if (i == null)
                        continue;

                    if (sprite.Serial == i.Serial)
                        continue;

                    if (i is Money)
                        continue;

                    var animation = new ServerFormat29
                    {
                        CasterSerial = (uint)sprite.Serial,
                        TargetSerial = (uint)i.Serial,
                        CasterEffect = (ushort)(10011),
                        TargetEffect = (ushort)(10011),
                        Speed = 100
                    };

                    //=$E$1*10 *$F$1 *G1 / 10
                    var dmg = sprite.Dex * 3 * ((int)sprite.Position.DistanceFrom(i.Position));

                    dmg *= count;
                    i.ApplyDamage(sprite, dmg, false, 28);

                    sprite.Show(Scope.NearbyAislings, animation);
                    cb?.Invoke(count++);
                }
            }
        }
    }
}
