using System;
using System.Collections.Generic;
using Marple.Core;
using ChronoDigger.Utils;

namespace ChronoDigger.Combat {
    /// <summary>Derived stats only update when <see cref="Calculate"/> is called.</summary>
    public class StatTable : AmountTable<StatDefinition> {
        public StatTable() {

        }

        /// <exception cref="ArgumentException">The profile lists a null stat or the same stat twice.</exception>
        public StatTable(StatProfile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            foreach (var entry in profile.Entries ?? new List<StatEntry>()) {
                AddEntry(entry.Stat, entry.BaseValue, profile);
            }
        }

        protected override double DefaultValue(StatDefinition stat) => stat.DefaultValue;

        /// <summary>
        /// Replaces every derivation with one computed from the stats' values without derivations.
        /// Derivations go one level deep: in A → B → C, C sees B without A's contribution.
        /// The result doesn't depend on order, and cycles are harmless.
        /// </summary>
        public void Calculate() {
            RemoveDerivations();
            var snapshot = Snapshot();

            var stats = new List<StatDefinition>(Definitions);
            for (int i = 0; i < stats.Count; i++) {
                var source = stats[i];
                if (source.Derivations == null) continue;

                Amount sourceValue = snapshot.GetValue(source);
                for (int d = 0; d < source.Derivations.Count; d++) {
                    var derivation = source.Derivations[d];
                    if (!Contains(derivation.Stat)) stats.Add(derivation.Stat);
                    // MultiplyScalar returns a new modifier, so the shared modifier is never changed.
                    var scaled = derivation.GetFullModifier(sourceValue);
                    Get(derivation.Stat).Add(source, d, scaled.With(source));
                }
            }
        }

        private void RemoveDerivations() {
            foreach (var source in Definitions) {
                if (source.Derivations == null) continue;

                for (int d = 0; d < source.Derivations.Count; d++) {
                    var target = source.Derivations[d].Stat;
                    if (Contains(target)) Get(target).Remove(source, d);
                }
            }
        }
    }
}
