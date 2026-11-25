using System;
using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat.Stats {
    /// <summary>
    /// The live stats of one actor: each <see cref="StatDefinition"/> it uses, paired with a
    /// <see cref="ModifiableAmount"/>. Built from a <see cref="StatProfile"/>; any stat without
    /// an entry reads as its default. Derived stats are only updated by <see cref="Calculate"/>.
    /// </summary>
    public class StatBlock {
        readonly Dictionary<StatDefinition, ModifiableAmount> _amounts = new Dictionary<StatDefinition, ModifiableAmount>();

        /// <exception cref="ArgumentException">The profile lists a null stat or the same stat twice.</exception>
        public StatBlock(StatProfile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            foreach (var entry in profile.overrides ?? new List<StatEntry>()) {
                if (entry?.stat == null) {
                    throw new ArgumentException($"Profile '{profile}' has an override with no stat.", nameof(profile));
                }
                if (_amounts.ContainsKey(entry.stat)) {
                    throw new ArgumentException($"Profile '{profile}' overrides '{entry.stat}' more than once.", nameof(profile));
                }
                _amounts.Add(entry.stat, new ModifiableAmount(entry.baseValue));
            }
        }

        /// <summary>The stat's current value, or its default if this block has no entry for it.</summary>
        public Amount Get(StatDefinition stat) {
            return _amounts.TryGetValue(stat, out var amount) ? amount.GetValue() : (Amount)stat.defaultValue;
        }

        /// <summary>The stat's ModifiableAmount, created from the default on first use.</summary>
        public ModifiableAmount GetAmount(StatDefinition stat) {
            if (stat == null) throw new ArgumentNullException(nameof(stat));
            if (!_amounts.TryGetValue(stat, out var amount)) {
                amount = new ModifiableAmount(stat.defaultValue);
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
                if (source.derivations == null) continue;

                Amount sourceValue = _amounts[source].GetValue();
                foreach (var derivation in source.derivations) {
                    if (!_amounts.ContainsKey(derivation.target)) stats.Add(derivation.target);

                    // MultiplyScalar returns a new modifier, so the shared perPoint is never changed.
                    // Keyed by the derivation, so each call replaces the previous value.
                    var modifier = derivation.perPoint.MultiplyScalar(sourceValue);
                    GetAmount(derivation.target).Add(derivation, modifier.With(source));
                }
            }
        }
    }
}
