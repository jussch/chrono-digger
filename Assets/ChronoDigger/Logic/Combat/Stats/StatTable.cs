using System;
using System.Collections.Generic;
using Marple.Core;
using ChronoDigger.Utils;

namespace ChronoDigger.Combat {
    /// <summary>Derived stats only update when <see cref="Calculate"/> is called.</summary>
    public class StatTable : AmountTable<StatDefinition> {
        /// <exception cref="ArgumentException">The profile lists a null stat or the same stat twice.</exception>
        public StatTable(StatProfile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            foreach (var entry in profile.Overrides ?? new List<StatEntry>()) {
                AddEntry(entry.Stat, entry.BaseValue, profile);
            }
        }

        protected override double DefaultValue(StatDefinition stat) => stat.DefaultValue;

        /// <summary>
        /// Applies every derivation of every stored stat to its target, replacing what the last
        /// call applied. A single pass: chains settle when each source is processed before its
        /// target, which holds for targets first created here.
        /// </summary>
        public void Calculate() {
            var stats = new List<StatDefinition>(Definitions);
            for (int i = 0; i < stats.Count; i++) {
                var source = stats[i];
                if (source.Derivations == null) continue;

                Amount sourceValue = Get(source);
                for (int d = 0; d < source.Derivations.Count; d++) {
                    var derivation = source.Derivations[d];
                    if (!Contains(derivation.Stat)) stats.Add(derivation.Stat);

                    // MultiplyScalar returns a new modifier, so the shared modifier is never changed.
                    // Keyed by source and index, so each call replaces the previous value.
                    var scaled = derivation.Modifier.MultiplyScalar(sourceValue);
                    GetAmount(derivation.Stat).Add(source, d, scaled.With(source));
                }
            }
        }
    }
}
