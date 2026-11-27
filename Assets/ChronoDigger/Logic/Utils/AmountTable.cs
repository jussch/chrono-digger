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

        public AmountTable<TDefinition> SnapShot() {
            var copy = (AmountTable<TDefinition>)MemberwiseClone();
            copy._amounts = new Dictionary<TDefinition, ModifiableAmount>();
            foreach (var pair in _amounts) {
                copy._amounts.Add(pair.Key, new ModifiableAmount(pair.Value.GetValue().ToDouble()));
            }
            return copy;
        }

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
    }
}
