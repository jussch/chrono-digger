using System;
using Marple.Core;

namespace ChronoDigger.Combat.Stats {
    /// <summary>
    /// "For each point of the owning stat, apply <see cref="modifier"/> to <see cref="stat"/>."
    /// Shared asset data: runtime code reads it and never changes it.
    /// </summary>
    [Serializable]
    public struct StatModifier {
        public StatDefinition stat;
        public AmountModifier modifier;

        public ModifiableAmount Retrieve(StatBlock statBlock) => statBlock.GetAmount(stat);
    }
}
