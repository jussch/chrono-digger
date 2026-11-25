using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat.Stats {
    /// <summary>
    /// Compared by reference, so there must be exactly one instance per stat.
    /// Construct only in <see cref="SampleStatData"/> (later: assets).
    /// </summary>
    // [CreateAssetMenu(menuName = "Combat/Stat Definition")]
    public class StatDefinition /* : ScriptableObject */ : IDescribable {
        public string displayName;
        public string description;
        public double defaultValue;
        public StatCategory category;

        // Odin: [ValidateInput(nameof(HasNoCycle), "Derivations form a cycle")]
        public List<StatModifier> derivations = new List<StatModifier>();

        public string DisplayName => displayName;
        public string Description => description;

        // private bool HasNoCycle(List<StatModifier> _) => StatDerivationValidator.FindCycle(this) == null;

        public override string ToString() => displayName;
    }
}
