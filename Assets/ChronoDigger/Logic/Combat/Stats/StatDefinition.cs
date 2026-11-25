using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat.Stats {
    /// <summary>
    /// One stat (MaxHealth, Wizardry, Fire Resistance...). Used as a dictionary key and
    /// compared by reference, so there must be exactly one instance per stat.
    /// Construct only in <see cref="SampleStatData"/> (later: assets).
    /// </summary>
    // [CreateAssetMenu(menuName = "Combat/Stat Definition")]
    public class StatDefinition /* : ScriptableObject */ : IDescribable {
        public string displayName;
        public string description;
        public double defaultValue;
        public StatCategory category;

        // Odin: [ValidateInput(nameof(HasNoCycle), "Derivations form a cycle")]
        public List<StatDerivation> derivations = new List<StatDerivation>();

        public string DisplayName => displayName;
        public string Description => description;

        // Odin validation hook (see derivations above).
        // private bool HasNoCycle(List<StatDerivation> _) => StatDerivationValidator.FindCycle(this) == null;

        public override string ToString() => displayName;
    }
}
