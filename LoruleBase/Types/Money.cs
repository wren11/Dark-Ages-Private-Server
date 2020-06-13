#region

using System;
using Darkages.Common;
using Darkages.Network.ServerFormats;

#endregion

namespace Darkages.Types
{
    public class Money : Sprite
    {
        public int Amount { get; set; }

        public MoneySprites Type { get; set; }

        public ushort Image { get; set; }

        public void GiveTo(int amount, Aisling aisling)
        {
            if (aisling.GoldPoints + amount < ServerContextBase.Config.MaxCarryGold)
            {
                aisling.GoldPoints += amount;

                if (aisling.GoldPoints > ServerContextBase.Config.MaxCarryGold)
                    aisling.GoldPoints = int.MaxValue;

                aisling.Client.SendMessage(0x03, $"You've Received {amount} coins.");
                aisling.Client.Send(new ServerFormat08(aisling, StatusFlags.StructC));

                Remove();
            }
        }

        public static void Create(Sprite parent, int amount, Position location)
        {
            if (parent == null)
                return;

            var money = new Money();
            money.CalcAmount(amount);

            lock (Generator.Random)
            {
                money.Serial = Generator.GenerateNumber();
            }

            money.AbandonedDate = DateTime.UtcNow;
            money.CurrentMapId = parent.CurrentMapId;
            money.XPos = location.X;
            money.YPos = location.Y;


            var mt = (int) money.Type;

            if (mt > 0) money.Image = (ushort) (mt + 0x8000);

            parent.AddObject(money);
        }

        private void CalcAmount(int amount)
        {
            Amount = amount;


            if (Amount > 0 && Amount < 10)
                Type = MoneySprites.SilverCoin;

            if (Amount >= 10 && Amount < 100)
                Type = MoneySprites.GoldCoin;

            if (Amount >= 100 && Amount < 1000)
                Type = MoneySprites.SilverPile;

            if (Amount >= 1000)
                Type = MoneySprites.GoldPile;
        }
    }
}