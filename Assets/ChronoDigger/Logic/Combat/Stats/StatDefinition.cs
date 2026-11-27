using System.Collections.Generic;
using Marple.Core;
using ChronoDigger.Utils;

namespace ChronoDigger.Combat {
    /// <summary>
    /// Compared by reference, so there must be exactly one instance per stat.
    /// Construct only in <see cref="SampleStatData"/> (later: assets).
    /// </summary>
    // [CreateAssetMenu(menuName = "Combat/Stat Definition")]
    public class StatDefinition : DescribableObject {
        public double DefaultValue;
        public StatCategory Category;

        // Odin: [ValidateInput(nameof(HasNoCycle), "Derivations form a cycle")]
        public List<StatModifier> Derivations = new List<StatModifier>();

        // private bool HasNoCycle(List<StatModifier> _) => StatDerivationValidator.FindCycle(this) == null;

        public override string ToString() => DisplayName;
    }
}
