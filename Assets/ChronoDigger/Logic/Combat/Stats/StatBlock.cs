using System;
using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat {
    /// <summary>
    /// A stat with no entry reads as its default; <see cref="GetAmount"/> creates the entry.
    /// Derived stats only update when <see cref="Calculate"/> is called.
    /// </summary>
    public class StatBlock {
        private readonly Dictionary<StatDefinition, ModifiableAmount> _amounts = new Dictionary<StatDefinition, ModifiableAmount>();

        /// <exception cref="ArgumentException">The profile lists a null stat or the same stat twice.</exception>
        public StatBlock(StatProfile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            foreach (var entry in profile.Overrides ?? new List<StatEntry>()) {
                if (entry?.Stat == null) {
                    throw new ArgumentException($"Profile '{profile}' has an override with no stat.", nameof(profile));
                }
                if (_amounts.ContainsKey(entry.Stat)) {
                    throw new ArgumentException($"Profile '{profile}' overrides '{entry.Stat}' more than once.", nameof(profile));
                }
                _amounts.Add(entry.Stat, new ModifiableAmount(entry.BaseValue));
            }
        }

        public Amount Get(StatDefinition stat) {
            return _amounts.TryGetValue(stat, out var amount) ? amount.GetValue() : (Amount)stat.DefaultValue;
        }

        public ModifiableAmount GetAmount(StatDefinition stat) {
            if (stat == null) throw new ArgumentNullException(nameof(stat));
            if (!_amounts.TryGetValue(stat, out var amount)) {
                amount = new ModifiableAmount(stat.DefaultValue);
                _amounts.Add(stat, amount);
            }
            return amount;
        }

        /// <summary>
        /// Applies every derivation of every stored stat to its target, replacing what the last
        /// call applied. A single pass: chains settle when each source is processed before its
        /// target, which holds for targets first created here.
        /// </summary>
        public void Calculate() {
            var stats = new List<StatDefinition>(_amounts.Keys);
            for (int i = 0; i < stats.Count; i++) {
                var source = stats[i];
                if (source.Derivations == null) continue;

                Amount sourceValue = _amounts[source].GetValue();
                for (int d = 0; d < source.Derivations.Count; d++) {
                    var derivation = source.Derivations[d];
                    if (!_amounts.ContainsKey(derivation.Stat)) stats.Add(derivation.Stat);

                    // MultiplyScalar returns a new modifier, so the shared modifier is never changed.
                    // Keyed by source and index, so each call replaces the previous value.
                    var scaled = derivation.Modifier.MultiplyScalar(sourceValue);
                    GetAmount(derivation.Stat).Add(source, d, scaled.With(source));
                }
            }
        }
    }
}
