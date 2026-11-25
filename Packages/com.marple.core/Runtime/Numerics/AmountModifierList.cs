using System;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
    [Serializable]
    public class AmountModifierList {
        public List<AmountModifier> modifiers;

        public AmountModifierList(IEnumerable<AmountModifier> modifiersN) {
            modifiers = modifiersN.ToList();
        }

        public AmountModifierList(params AmountModifier[] modifiersN) {
            modifiers = modifiersN.ToList();
        }

        public AmountModifierRecord GetRecord(object source, IDescribable describable = null, TextState state = default) {
            var record = new AmountModifierRecord();
            for (int i = 0; i < modifiers.Count; i++) {
                record.Add(source, i, new AmountModifierRecord.Entry(modifiers[i], describable, state));
            }
            return record;
        }

        public Amount Adjust(Amount value) => AmountModifier.AdjustWith(value, modifiers);

        public AmountModifier Adjust(AmountModifier value) => AmountModifier.AdjustWith(value, modifiers);

        public AmountModifierList MultiplyScalar(Amount scalar) {
            return new AmountModifierList(modifiers.Select(mod => mod.MultiplyScalar(scalar)));
        }

        // Applies ModifierList multiple times and sums up the values.
        // public Amount SumMultiplyScalar(Amount value, int finalExclusive, int start = 0) {
        //     Amount sum = 0;
        //     for (int i = start; i < finalExclusive; i++) {
        //         sum += MultiplyScalar(i).Adjust(value);
        //     }
        //
        //     return sum;
        // }

        // This version uses a top-down approach and stops when the change is too small.
        public Amount SumMultiplyScalar(Amount value, int finalExclusive, int start = 0) {
            Amount sum = 0;
            for (int i = finalExclusive - 1; i >= start; i--) {
                var val = MultiplyScalar(i).Adjust(value);
                if (val.Exponent <= sum.Exponent - 6 || val.IsNaN) break;
                sum += val;
            }

            return sum;
        }

        public bool IsEmpty() => modifiers.Count == 0 || modifiers.All(mod => mod.IsEmpty());
        public bool HasMultipleMods() => modifiers.Count > 1;

        public override string ToString() {
            return ToStringRaw();
        }

        public string ToStringRaw(bool useSpace = true) {
            return string.Join(" ", modifiers.Select(mod => mod.ToStringRaw(useSpace)));
        }

        public static List<AmountModifier> Flatten(params AmountModifierList[] modifierLists) {
            return modifierLists.SelectMany(modList => modList.modifiers).ToList();
        }
    }
}
