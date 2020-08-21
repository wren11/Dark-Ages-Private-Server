#region

using System.Collections.Generic;
using System.ComponentModel;

#endregion

namespace Darkages.Types
{
    public class SkillTemplate : Template
    {
        public Buff Buff { get; set; }
        public int Cooldown { get; set; }
        public Debuff Debuff { get; set; }
        public string FailMessage { get; set; }
        public byte Icon { get; set; }

        public List<LearningPredicate> LearningRequirements { get; set; } = new List<LearningPredicate>();
        public double LevelRate { get; set; }
        public int MaxLevel { get; set; }
        public ushort MissAnimation { get; set; }
        public string NpcKey { get; set; }
        public Pane Pane { get; set; }
        public PostQualifer PostQualifers { get; set; }
        public LearningPredicate Prerequisites { get; set; }
        public string ScriptName { get; set; }
        public byte Sound { get; set; }
        public ushort TargetAnimation { get; set; }
        public Tier TierLevel { get; set; }
        public SkillScope Type { get; set; }

        public override string[] GetMetaData()
        {
            if (Prerequisites != null) return Prerequisites.MetaData;

            return default;
        }
    }
}