using System;

namespace ChronoDigger.Combat.Stats {
    /// <summary>One override in a <see cref="StatProfile"/>.</summary>
    [Serializable]
    public class StatEntry {
        public StatDefinition stat;
        public double baseValue;
    }
}
