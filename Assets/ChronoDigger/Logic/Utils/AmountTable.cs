using System;
using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Utils {
    /// <summary>
    /// Runtime amounts keyed by definition. A definition with no entry reads as its default;
    /// <see cref="Get"/> creates the entry.
    /// </summary>
    public abstract class AmountTable<TDefinition> where TDefinition : class {
        private Dictionary<TDefinition, ModifiableAmount> _amounts = new Dictionary<TDefinition, ModifiableAmount>();

        protected IEnumerable<TDefinition> Definitions => _amounts.Keys;

        protected abstract double DefaultValue(TDefinition definition);

        public ModifiableAmount Get(TDefinition definition) {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (!_amounts.TryGetValue(definition, out var amount)) {
                amount = new ModifiableAmount(DefaultValue(definition));
                _amounts.Add(definition, amount);
            }
            return amount;
        }

        public Amount GetValue(TDefinition definition) {
            return _amounts.TryGetValue(definition, out var amount) ? amount.GetValue() : (Amount)DefaultValue(definition);
        }

        public AmountTable<TDefinition> Snapshot() {
            var copy = (AmountTable<TDefinition>)MemberwiseClone();
            copy._amounts = new Dictionary<TDefinition, ModifiableAmount>();
            foreach (var pair in _amounts) {
                copy._amounts.Add(pair.Key, new ModifiableAmount(pair.Value.GetValue().ToDouble()));
            }
            return copy;
        }

        /// <summary>
        /// Copies every entry of <paramref name="other"/> into this table, modifiers included.
        /// Where both tables have an entry, <paramref name="resolveConflict"/> decides what this table keeps;
        /// see <see cref="AmountTableConflicts"/> for the common choices.
        /// <paramref name="other"/> is never changed, and the two tables share no amounts afterwards.
        /// </summary>
        public void Merge(AmountTable<TDefinition> other, ConflictResolver resolveConflict) {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (resolveConflict == null) throw new ArgumentNullException(nameof(resolveConflict));

            foreach (var pair in other._amounts) {
                var incoming = Copy(pair.Value);
                if (_amounts.TryGetValue(pair.Key, out var existing)) {
                    _amounts[pair.Key] = resolveConflict(pair.Key, existing, incoming);
                } else {
                    _amounts.Add(pair.Key, incoming);
                }
            }
        }

        /// <param name="incoming">A copy, so it's safe to return or keep.</param>
        /// <returns>The amount this table keeps for <paramref name="definition"/>.</returns>
        public delegate ModifiableAmount ConflictResolver(TDefinition definition, ModifiableAmount existing,
            ModifiableAmount incoming);

        protected bool Contains(TDefinition definition) => _amounts.ContainsKey(definition);

        /// <summary>For seeding from a profile.</summary>
        /// <exception cref="ArgumentException">The definition is null or already has an entry.</exception>
        protected void AddEntry(TDefinition definition, double baseValue, object profile) {
            if (definition == null) {
                throw new ArgumentException($"Profile '{profile}' has an entry with no definition.", nameof(profile));
            }
            if (_amounts.ContainsKey(definition)) {
                throw new ArgumentException($"Profile '{profile}' lists '{definition}' more than once.", nameof(profile));
            }
            _amounts.Add(definition, new ModifiableAmount(baseValue));
        }

        /**
         * Statics
         */
        private static ModifiableAmount Copy(ModifiableAmount amount) {
            var copy = new ModifiableAmount(amount.BaseValue);
            copy.Add(amount);
            return copy;
        }

        public static TCombine CombineSnapshot<TCombine>(TCombine table1, TCombine table2) where TCombine : AmountTable<TDefinition> {
            if (table1 == null && table2 == null) return null;
            if (table1 == null) return (TCombine)table2.Snapshot();
            if (table2 == null) return (TCombine)table1.Snapshot();
            
            AmountTable<TDefinition> returnTable = table1.Snapshot();
            returnTable.Merge(table2.Snapshot(), AmountTableConflicts.Sum);
            return (TCombine)returnTable;
        }
    }
}
