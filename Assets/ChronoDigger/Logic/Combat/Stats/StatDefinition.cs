using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat {
    /// <summary>
    /// Compared by reference, so there must be exactly one instance per stat.
    /// Construct only in <see cref="SampleStatData"/> (later: assets).
    /// </summary>
    // [CreateAssetMenu(menuName = "Combat/Stat Definition")]
    public class StatDefinition /* : ScriptableObject */ : IDescribable {
        // [field: SerializeField] on these two in Unity: they're auto-properties because IDescribable needs properties.
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public double DefaultValue;
        public StatCategory Category;

        // Odin: [ValidateInput(nameof(HasNoCycle), "Derivations form a cycle")]
        public List<StatModifier> Derivations = new List<StatModifier>();

        // private bool HasNoCycle(List<StatModifier> _) => StatDerivationValidator.FindCycle(this) == null;

        public override string ToString() => DisplayName;
    }
}
