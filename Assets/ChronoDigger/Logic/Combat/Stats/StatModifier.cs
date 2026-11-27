using System;
using Marple.Core;

namespace ChronoDigger.Combat {
    /// <summary>
    /// "For each point of the owning stat, apply <see cref="modifier"/> to <see cref="stat"/>."
    /// Shared asset data: runtime code reads it and never changes it.
    /// </summary>
    [Serializable]
    public struct StatModifier {
        public StatDefinition Stat;
        public AmountModifier Modifier;

        public ModifiableAmount Retrieve(StatTable statTable) => statTable.GetAmount(Stat);
    }
}
