using System;
using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat.Stats {
    /// <summary>
    /// The live stats of one actor, built from a <see cref="StatProfile"/>. Stores a
    /// <see cref="ModifiableAmount"/> only for stats that need one (profile overrides, stats
    /// someone modified or observed, and derivation targets); every other stat reads as its default.
    ///
    /// Derived stats are pull-based: reading a stat first brings its derivation sources up to
    /// date (recursively, so chains work), re-applying a derivation only if its source changed.
    /// Read values through <see cref="Get"/> or <see cref="Observe"/>, not the ModifiableAmount
    /// directly, so derivations are refreshed first.
    /// </summary>
    public class StatBlock {
        const int MaxDerivationDepth = 32;

        // A derivation together with the stat that owns it.
        readonly struct Feed {
            public readonly StatDefinition Source;
            public readonly StatDerivation Derivation;

            public Feed(StatDefinition source, StatDerivation derivation) {
                Source = source;
                Derivation = derivation;
            }
        }

        readonly Dictionary<StatDefinition, double> _overrides = new Dictionary<StatDefinition, double>();
        readonly Dictionary<StatDefinition, ModifiableAmount> _amounts = new Dictionary<StatDefinition, ModifiableAmount>();
        readonly Dictionary<StatDefinition, List<Feed>> _feedsByTarget = new Dictionary<StatDefinition, List<Feed>>();
        readonly Dictionary<StatDerivation, int> _appliedAtSourceId = new Dictionary<StatDerivation, int>();
        readonly Dictionary<StatDefinition, StatObserver> _observers = new Dictionary<StatDefinition, StatObserver>();

        /// <exception cref="ArgumentException">The profile lists a null stat or the same stat twice.</exception>
        public StatBlock(StatProfile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            foreach (var entry in profile.overrides ?? new List<StatEntry>()) {
                if (entry?.stat == null) {
                    throw new ArgumentException($"Profile '{profile}' has an override with no stat.", nameof(profile));
                }
                if (_overrides.ContainsKey(entry.stat)) {
                    throw new ArgumentException($"Profile '{profile}' overrides '{entry.stat}' more than once.", nameof(profile));
                }
                _overrides.Add(entry.stat, entry.baseValue);
            }

            foreach (var stat in _overrides.Keys) GetOrCreateAmount(stat);
        }

        /// <summary>The stat's final value, with all modifiers and derivations applied.</summary>
        public Amount Get(StatDefinition stat) {
            if (!_amounts.TryGetValue(stat, out var amount)) return GetBaseValue(stat);
            Refresh(stat, 0);
            return amount.GetValue();
        }

        /// <summary>The stat's ModifiableAmount, for adding and removing equipment or buff modifiers.</summary>
        public ModifiableAmount GetAmount(StatDefinition stat) {
            var amount = GetOrCreateAmount(stat);
            Refresh(stat, 0);
            return amount;
        }

        /// <summary>Changes whenever the stat's final value may have changed, including through derivations.</summary>
        public IPassiveObservable Observe(StatDefinition stat) {
            if (!_observers.TryGetValue(stat, out var observer)) {
                GetOrCreateAmount(stat);
                observer = new StatObserver(this, stat);
                _observers.Add(stat, observer);
            }
            return observer;
        }

        double GetBaseValue(StatDefinition stat) {
            return _overrides.TryGetValue(stat, out double value) ? value : stat.defaultValue;
        }

        ModifiableAmount GetOrCreateAmount(StatDefinition stat) {
            if (stat == null) throw new ArgumentNullException(nameof(stat));
            if (_amounts.TryGetValue(stat, out var amount)) return amount;

            amount = new ModifiableAmount(GetBaseValue(stat));
            // Stored before following derivations so a cycle finds it instead of recursing here.
            _amounts.Add(stat, amount);

            if (stat.derivations != null) {
                foreach (var derivation in stat.derivations) {
                    if (derivation?.target == null) {
                        throw new InvalidOperationException($"Stat '{stat}' has a derivation with no target.");
                    }
                    GetOrCreateAmount(derivation.target);
                    if (!_feedsByTarget.TryGetValue(derivation.target, out var feeds)) {
                        feeds = new List<Feed>();
                        _feedsByTarget.Add(derivation.target, feeds);
                    }
                    feeds.Add(new Feed(stat, derivation));
                }
            }
            return amount;
        }

        // Re-applies every derivation feeding the target whose source changed since it was last applied.
        void Refresh(StatDefinition target, int depth) {
            if (!_feedsByTarget.TryGetValue(target, out var feeds)) return;
            if (depth > MaxDerivationDepth) {
                throw new InvalidOperationException(
                    $"Stat derivations nest deeper than {MaxDerivationDepth} at '{target}'; they probably form a cycle. " +
                    "Use StatDerivationValidator.FindCycle to find it.");
            }

            foreach (var feed in feeds) {
                Refresh(feed.Source, depth + 1);

                var source = _amounts[feed.Source];
                Amount sourceValue = source.GetValue(); // settle the value first: calculating can bump the id
                int sourceId = source.GetObservableId();
                if (_appliedAtSourceId.TryGetValue(feed.Derivation, out int appliedId) && appliedId == sourceId) continue;

                // MultiplyScalar returns a new modifier, so the shared perPoint is never changed.
                // Same key every time, so this replaces the previous value instead of stacking.
                var modifier = feed.Derivation.perPoint.MultiplyScalar(sourceValue);
                _amounts[target].Add(feed.Derivation, modifier.With(feed.Source));
                _appliedAtSourceId[feed.Derivation] = sourceId;
            }
        }

        sealed class StatObserver : IPassiveObservable {
            readonly StatBlock _block;
            readonly StatDefinition _stat;

            public StatObserver(StatBlock block, StatDefinition stat) {
                _block = block;
                _stat = stat;
            }

            public int GetObservableId() {
                _block.Refresh(_stat, 0);
                var amount = _block._amounts[_stat];
                amount.GetValue();
                return amount.GetObservableId();
            }
        }
    }
}
