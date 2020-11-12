#region

using System;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Storage.locales.Scripts.Weapons
{
    [Script("Snow Secret", "Dean")]
    public class SnowSecret : WeaponScript
    {
        public SnowSecret(Item item) : base(item)
        {
        }

        public override void OnUse(Sprite sprite, Action<int> cb = null)
        {
            var enemy = sprite.GetInfront(8);
            var count = 1;
            if (enemy != null)
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
                        CasterSerial = (uint) sprite.Serial,
                        TargetSerial = (uint) i.Serial,
                        CasterEffect = 10011,
                        TargetEffect = 10011,
                        Speed = 100
                    };

                    var dmg = sprite.Dex * 3 * sprite.Position.DistanceFrom(i.Position);

                    dmg *= count;
                    i.ApplyDamage(sprite, dmg, 28);

                    sprite.Show(Scope.NearbyAislings, animation);
                    cb?.Invoke(count++);
                }
        }
    }
}