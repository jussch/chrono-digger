using System;
using Marple.Core;

namespace ChronoDigger.Combat {
    /// <summary>
    /// "For each point of the owning stat, apply <see cref="modifier"/> to <see cref="stat"/>."
    /// Shared asset data: runtime code reads it and never changes it.
    /// <see cref="Stat"> can be the Target Stat or the Source Stat.
    /// </summary>
    [Serializable]
    public struct StatModifier {
        public StatDefinition Stat;
        public AmountModifier Modifier;
        public int Fraction;
        public int MaxMultiplier;

        public ModifiableAmount RetrieveStatFromTable(StatTable statTable) => statTable.Get(Stat);
        /// <param name="sourceValue">The value of the stat that owns this modifier, which isn't always <see cref="Stat"/>.</param>
        public int GetMultiplier(Amount sourceValue) => ClampMultiplier(sourceValue.ToInt() / ClampFraction());
        /// <param name="sourceValue">The value of the stat that owns this modifier, which isn't always <see cref="Stat"/>.</param>
        public AmountModifier GetFullModifier(Amount sourceValue) => Modifier.MultiplyScalar(GetMultiplier(sourceValue));

        private int ClampFraction() => Math.Max(1, Fraction);
        private int ClampMultiplier(int mult) => MaxMultiplier > 0 ? Math.Min(mult, MaxMultiplier) : mult;
    }
}
