using System.Collections.Generic;
using ChronoDigger.Combat.Stats;

namespace ChronoDigger.Combat.Actors {
    public class StatusEffectDefinition /*: ScriptableObject */ {
        public int maxStacks = 0;
        public int baseTime = 10;

        public bool stacksMultiplyModifiers;
        public List<StatModifier> statModifiers;
    }
}
