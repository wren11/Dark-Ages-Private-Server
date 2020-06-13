namespace Darkages.Types
{
    public class SpellTemplate : Template
    {
        public enum SpellUseType : byte
        {
            Unusable = 0,
            Prompt = 1,
            ChooseTarget = 2,
            FourDigit = 3,
            ThreeDigit = 4,
            NoTarget = 5,
            TwoDigit = 6,
            OneDigit = 7
        }

        public SpellTemplate()
        {
            Text = string.Empty + "\0";
        }

        public byte Icon { get; set; }
        public byte MaxLevel { get; set; }
        public string ScriptKey { get; set; }
        public int MinLines { get; set; }
        public int MaxLines { get; set; }
        public int ManaCost { get; set; }
        public string Text { get; set; }
        public Debuff Debuff { get; set; }
        public Buff Buff { get; set; }
        public SpellUseType TargetType { get; set; }
        public int BaseLines { get; set; }
        public double LevelRate { get; set; }
        public byte Sound { get; set; }
        public ushort Animation { get; set; }
        public double DamageExponent { get; set; }
        public Pane Pane { get; set; }
        public int Cooldown { get; set; } = 0;
        public ElementManager.Element ElementalProperty { get; set; }
        public LearningPredicate Prerequisites { get; set; }
        public Tier TierLevel { get; set; }
        public string NpcKey { get; set; }
        public ushort TargetAnimation { get; set; }
        public bool IsTrap { get; set; }
    }
}