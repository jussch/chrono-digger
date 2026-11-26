using System;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
    [Serializable]
    public class AmountModifierList {
        public List<AmountModifier> Modifiers;

        public AmountModifierList(IEnumerable<AmountModifier> modifiersN) {
            Modifiers = modifiersN.ToList();
        }

        public AmountModifierList(params AmountModifier[] modifiersN) {
            Modifiers = modifiersN.ToList();
        }

        public AmountModifierRecord GetRecord(object source, IDescribable describable = null, TextState state = default) {
            var record = new AmountModifierRecord();
            for (int i = 0; i < Modifiers.Count; i++) {
                record.Add(source, i, new AmountModifierRecord.Entry(Modifiers[i], describable, state));
            }
            return record;
        }

        public Amount Adjust(Amount value) => AmountModifier.AdjustWith(value, Modifiers);

        public AmountModifier Adjust(AmountModifier value) => AmountModifier.AdjustWith(value, Modifiers);

        public AmountModifierList MultiplyScalar(Amount scalar) {
            return new AmountModifierList(Modifiers.Select(mod => mod.MultiplyScalar(scalar)));
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

        public bool IsEmpty() => Modifiers.Count == 0 || Modifiers.All(mod => mod.IsEmpty());
        public bool HasMultipleMods() => Modifiers.Count > 1;

        public override string ToString() {
            return ToStringRaw();
        }

        public string ToStringRaw(bool useSpace = true) {
            return string.Join(" ", Modifiers.Select(mod => mod.ToStringRaw(useSpace)));
        }

        public static List<AmountModifier> Flatten(params AmountModifierList[] modifierLists) {
            return modifierLists.SelectMany(modList => modList.Modifiers).ToList();
        }
    }
}
