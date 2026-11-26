using System.Collections.Generic;

namespace ChronoDigger.Combat {
    public class StatusEffectDefinition /*: ScriptableObject */ {
        public int MaxStacks = 0;
        public int BaseTime = 10;

        public bool StacksMultiplyModifiers;
        public List<StatModifier> StatModifiers;
    }
}
