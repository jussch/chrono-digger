using System;
using Marple.Core;

namespace ChronoDigger.Utils {
    /// <summary>Ready-made conflict resolvers for <see cref="AmountTable{TDefinition}.Merge"/>.</summary>
    public static class AmountTableConflicts {
        /// <exception cref="InvalidOperationException">Always. Entries merged before the conflict stay merged.</exception>
        public static ModifiableAmount Throw<TDefinition>(TDefinition definition, ModifiableAmount existing,
                ModifiableAmount incoming) {
            throw new InvalidOperationException($"Both tables have an entry for '{definition}'.");
        }

        public static ModifiableAmount KeepExisting<TDefinition>(TDefinition definition, ModifiableAmount existing,
                ModifiableAmount incoming) {
            return existing;
        }

        public static ModifiableAmount KeepIncoming<TDefinition>(TDefinition definition, ModifiableAmount existing,
                ModifiableAmount incoming) {
            return incoming;
        }

        /// <summary>
        /// Keeps the existing amount and adds the incoming base value as a raw modifier, plus all incoming modifiers.
        /// An incoming modifier with the same key as an existing one replaces it.
        /// </summary>
        public static ModifiableAmount Sum<TDefinition>(TDefinition definition, ModifiableAmount existing,
                ModifiableAmount incoming) {
            existing.Combine(incoming);
            return existing;
        }
    }
}
