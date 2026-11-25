using System;
using Marple.Core;

namespace ChronoDigger.Combat.Stats {
    /// <summary>
    /// "For each point of the owning stat, apply <see cref="perPoint"/> to <see cref="target"/>."
    /// Shared asset data: runtime code reads it and never changes it.
    /// </summary>
    [Serializable]
    public class StatModifier {
        public StatDefinition target;
        public AmountModifier perPoint;
    }
}
