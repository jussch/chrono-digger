using System.Collections.Generic;

namespace ChronoDigger.Combat {
    public class StatusEffectDefinition /*: ScriptableObject */ {
        public int maxStacks = 0;
        public int baseTime = 10;

        public bool stacksMultiplyModifiers;
        public List<StatModifier> statModifiers;
    }
}
