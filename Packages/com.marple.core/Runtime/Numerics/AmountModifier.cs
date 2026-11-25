using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
    [Serializable]
    public struct AmountModifier : IEquatable<AmountModifier> {
        public enum AmountModifierType {
            Raw,
            MultiDiminishing,
            MultiStacking,
            Reduction,
            Exponent,
        }

        // [OnValueChanged("UpdateRealAdjust")]
        // [SerializeField]
        private Amount adjust;
        
        // [OnValueChanged("UpdateRealAdjust")]
        // [SerializeField]
        private AmountModifierType type;

        // [SerializeField]
        private int order;

        private Amount adjustReal;

        public AmountModifierType Type => type;
        public int Order => order;

        public AmountModifier(Amount adjustN, AmountModifierType typeN) {
            adjust = adjustN.ToDouble();
            adjustReal = adjustN;
            type = typeN;
            order = 0;
            if (IsOneDefault()) adjust = adjustReal.ToDouble() - 1;
        }
        
        public AmountModifier(AmountModifierType typeN) {
            type = typeN;
            adjustReal = Amount.Zero;
            adjust = 0;
            order = 0;
            if (IsOneDefault()) adjustReal = Amount.One;
            
        }

        public AmountModifier(AmountModifier amountModifier) {
            type = amountModifier.type;
            adjust = amountModifier.adjust;
            adjustReal = amountModifier.adjustReal;
            order = amountModifier.order;
        }

        public AmountModifier(AmountModifier amountModifier, Amount newValue) {
            type = amountModifier.type;
            adjust = newValue.ToDouble();
            adjustReal = newValue;
            order = amountModifier.order;
        }

        public static implicit operator string(AmountModifier mod) => mod.ToString();
        public static implicit operator bool(AmountModifier mod) => !mod.IsEmpty();
        
        public bool IsEmpty() {
            switch (type) {
                case AmountModifierType.MultiStacking: 
                case AmountModifierType.Exponent: 
                    // return Utils.Approx(adjustReal, 1); <-- This didn't allow for very small numbers.
                    return adjustReal == 1;
                case AmountModifierType.Raw: 
                case AmountModifierType.MultiDiminishing: 
                case AmountModifierType.Reduction: 
                    // return Utils.Approx(adjustReal, 0); <-- This didn't allow for very small numbers.
                    return adjustReal == 0;
                default: return true;
            }
        }

        public bool IsNearZero() {
            return adjustReal.Exponent < -5;
        }

        public bool IsNaN() {
            return adjustReal.IsNaN;
        }

        // public int Adjust(int value) {
        //     return (int)Math.Floor(Adjust((double) value));
        // }
        //
        // public long Adjust(long value) {
        //     return (long)Math.Floor(Adjust((double) value));
        // }

        // public double Adjust(double value) => Adjust(new Amount(value)).ToDouble();
        public Amount Adjust(Amount value) {
            switch (type) {
                case AmountModifierType.Raw: return value + adjustReal;
                case AmountModifierType.MultiDiminishing: return value * (1 + adjustReal);
                case AmountModifierType.MultiStacking: return value * adjustReal;
                case AmountModifierType.Reduction: return value / (1 + adjustReal);
                case AmountModifierType.Exponent: return Amount.Pow(value, value > 0 && value < 1  ? adjustReal.Invert() : adjustReal);
                default: return value;
            }
        }
        
        public AmountModifier Adjust(AmountModifier modifier) {
            switch (modifier.type) {
                case AmountModifierType.MultiStacking: return new AmountModifier(modifier, modifier.adjustReal.Pow(Adjust(Amount.One)));
                case AmountModifierType.Exponent: return new AmountModifier(modifier, modifier.adjustReal.Pow(Adjust(Amount.One)));
                case AmountModifierType.Raw:
                case AmountModifierType.MultiDiminishing: 
                case AmountModifierType.Reduction: 
                    return new AmountModifier(modifier, Adjust(modifier.adjustReal));
                default: return modifier;
            }
        }

        // Note: in sequence adjust (nStart of 0 = value).
        public Amount SumSequenceAdjustBetween(Amount value, Amount nStart, Amount nEnd) => 
            SumSequenceAdjust(value, nEnd) - SumSequenceAdjust(value, nStart);
        public Amount SumSequenceAdjust(Amount value, Amount n) {
            if (n < 0) return 0;
            if (IsEmpty()) return value * (n+1);
            switch (type) {
                case AmountModifierType.Raw: return Utils.SumArithmeticSequenceDiff(value, adjustReal, n);
                case AmountModifierType.MultiDiminishing: return Utils.SumArithmeticSequenceDiff(value, value * adjustReal, n);
                case AmountModifierType.MultiStacking: return Utils.SumGeometricSequence(value, adjustReal, n);
                case AmountModifierType.Reduction: throw new Exception("SequenceAdjust of Reduction modifiers not implemented.");
                case AmountModifierType.Exponent: throw new Exception("SequenceAdjust of Exponent modifiers not implemented.");
                default: return value;
            }
        }

        public AmountModifier SumSequenceAdjust(AmountModifier modifier, Amount n) {
            switch (modifier.type) {
                case AmountModifierType.Raw:
                case AmountModifierType.MultiDiminishing:
                case AmountModifierType.Reduction:
                    return new AmountModifier(modifier, SumSequenceAdjust(modifier.adjustReal, n));
                case AmountModifierType.MultiStacking:
                case AmountModifierType.Exponent:
                    return new AmountModifier(modifier, modifier.adjustReal.Pow(SumSequenceAdjust(1, n)));
                default: return modifier;
            }
        }

        // Will return 0 = starting value.
        public Amount FindSequenceN(Amount sum, Amount start, int startN = 0) {
            if (IsEmpty()) return Amount.Max((sum / start) - startN, 0);
            Amount actualStart = startN <= 0 ? start : MultiplyScalar(startN).Adjust(start);
            if (sum < actualStart) return 0;
            switch (type) {
                case AmountModifierType.Raw: return Utils.FindArithmeticSequenceDiffN(sum, actualStart, adjustReal);
                case AmountModifierType.MultiDiminishing: return Utils.FindArithmeticSequenceDiffN(sum, actualStart, start * adjustReal);
                case AmountModifierType.MultiStacking: return Utils.FindGeometricSequenceN(sum, actualStart, adjustReal) + 1;
                case AmountModifierType.Reduction: throw new Exception("FindSequenceN of Reduction modifiers not implemented.");
                case AmountModifierType.Exponent: throw new Exception("FindSequenceN of Exponent modifiers not implemented.");
                default: return sum / start;
            }
        }

        public AmountModifier AdjustWith(AmountModifier modifier) {
            return modifier.Adjust(this);
        }

        public AmountModifier Apply(AmountModifier modifier) {
            return modifier.Adjust(this);
        }

        public AmountModifier Apply(AmountModifierRecord modifierRecord) {
            return modifierRecord.Adjust(this);
        }

        public AmountModifier Apply(AmountModifierRecord modifierRecord, Amount ratio) {
            return modifierRecord.Adjust(MultiplyScalar(ratio));
        }

        public AmountModifier GetChange(AmountModifier modifier) {
            AmountModifier changed = Apply(modifier);
            return new AmountModifier(this, changed.adjustReal - adjustReal);
        }

        public AmountModifier GetChange(AmountModifierRecord record, AmountModifier modifier) {
            AmountModifier changed = Apply(modifier);
            return new AmountModifier(this, changed.adjustReal - adjustReal);
        }

        public AmountModifier AddModifier(AmountModifier modifier) {
            switch (type) {
                case AmountModifierType.Raw: return new AmountModifier(this, adjustReal + modifier.adjustReal);
                case AmountModifierType.MultiDiminishing: return new AmountModifier(this, adjustReal + modifier.adjustReal);
                case AmountModifierType.MultiStacking: return new AmountModifier(this, adjustReal * modifier.adjustReal);
                case AmountModifierType.Reduction: return new AmountModifier(this, adjustReal + modifier.adjustReal);
                case AmountModifierType.Exponent: return new AmountModifier(this, adjustReal * modifier.adjustReal);
                default: return this;
            }
        }

        public readonly AmountModifier MultiplyScalar(Amount scalar) {
            switch (type) {
                case AmountModifierType.Raw: return new AmountModifier(this, adjustReal * scalar);
                case AmountModifierType.MultiDiminishing: return new AmountModifier(this, adjustReal * scalar);
                case AmountModifierType.MultiStacking: return new AmountModifier(this, adjustReal.Pow(scalar));
                case AmountModifierType.Reduction: return new AmountModifier(this, adjustReal * scalar);
                case AmountModifierType.Exponent: return new AmountModifier(this, adjustReal.Pow(scalar));
                default: return this;
            }
        }

        public AmountModifier PowerScalar(Amount scalar) {
            switch (type) {
                case AmountModifierType.Raw: return new AmountModifier(this, Amount.Pow(adjustReal, scalar));
                case AmountModifierType.MultiDiminishing: return new AmountModifier(this, Amount.Pow(adjustReal + 1, scalar) - 1);
                case AmountModifierType.MultiStacking: return new AmountModifier(this, adjustReal.Pow(scalar));
                case AmountModifierType.Reduction: return new AmountModifier(this, Amount.Pow(adjustReal + 1, scalar) - 1);
                case AmountModifierType.Exponent: return new AmountModifier(this, adjustReal.Pow(scalar));
                default: return this;
            }
        }

        public Amount SumMultiplyScalar(Amount value, Amount scalar) {
            Amount sum = 0;
            for (int i = 0; i < scalar; i++) {
                sum += MultiplyScalar(i).Adjust(value);
            }

            return sum;
        }

        public AmountModifier SetOrder(int orderN) {
            return new AmountModifier(this) {order = orderN};
        }

        public AmountModifier AdjustOrder(int orderN) {
            return SetOrder(orderN + order);
        }

        public bool IsCompatible(AmountModifier modifier) {
            return type == modifier.type && order == modifier.order;
        }

        public static List<AmountModifier> SortList(IEnumerable<AmountModifier> modifiers) {
            List<AmountModifier> newList = modifiers.Select(v => v).ToList();
            
            newList.Sort((v1, v2) => v1.order.CompareTo(v2.order));
            
            return newList;
        }

        public static List<AmountModifier> SortListType(IEnumerable<AmountModifier> modifiers) {
            List<AmountModifier> newList = modifiers.Select(v => v).ToList();
            
            newList.Sort((v1, v2) => Utils.SortCompare(v1.order.CompareTo(v2.order), v1.type.CompareTo(v2.type)));
            
            return newList;
        }

        // Assumes same order
        public static List<AmountModifier> CombineSameOrderList(List<AmountModifier> modifiers) {
            if (modifiers.Count == 0) return new List<AmountModifier>();
            var order = modifiers[0].order;
            AmountModifier rawModifier = new AmountModifier(AmountModifierType.Raw);
            AmountModifier bonusModifier = new AmountModifier(AmountModifierType.MultiDiminishing);
            AmountModifier multModifier = new AmountModifier(AmountModifierType.MultiStacking);
            AmountModifier redModifier = new AmountModifier(AmountModifierType.Reduction);
            AmountModifier expModifier = new AmountModifier(AmountModifierType.Exponent);
            foreach (AmountModifier modifier in modifiers) {
                switch (modifier.type) {
                    case AmountModifierType.Raw: rawModifier = rawModifier.AddModifier(modifier);
                        break;
                    case AmountModifierType.MultiDiminishing: bonusModifier = bonusModifier.AddModifier(modifier);
                        break;
                    case AmountModifierType.MultiStacking: multModifier = multModifier.AddModifier(modifier);
                        break;
                    case AmountModifierType.Reduction: redModifier = redModifier.AddModifier(modifier);
                        break;
                    case AmountModifierType.Exponent: expModifier = expModifier.AddModifier(modifier);
                        break;
                    default: continue;
                }
            }

            List<AmountModifier> combinedList = new List<AmountModifier>();
            if (!rawModifier.IsEmpty()) combinedList.Add(rawModifier.SetOrder(order));
            if (!bonusModifier.IsEmpty()) combinedList.Add(bonusModifier.SetOrder(order));
            if (!multModifier.IsEmpty()) combinedList.Add(multModifier.SetOrder(order));
            if (!redModifier.IsEmpty()) combinedList.Add(redModifier.SetOrder(order));
            if (!expModifier.IsEmpty()) combinedList.Add(expModifier.SetOrder(order));
            return combinedList;
        }

        public string ToStringValue(bool useSpace = true) {
            switch (type) {
                case AmountModifierType.Raw: return (adjustReal >= 0 ? "+" : "-") + Amount.Abs(adjustReal).Beautify();
                case AmountModifierType.MultiDiminishing: return (adjustReal >= 0 ? "+" : "-") + Amount.Abs(adjustReal * 100).Beautify() + "%";
                case AmountModifierType.MultiStacking: return  "x" + Amount.Abs(adjustReal).Beautify();
                case AmountModifierType.Reduction: return  (adjustReal >= 0 ? "-" : "+") + Amount.Abs(100 * adjustReal).Beautify() + "%";
                case AmountModifierType.Exponent: return  "^" + Amount.Abs(adjustReal).Beautify();
                default: return adjustReal.ToString();
            }
        }
        
        public string ToStringRaw(bool useSpace = true) {
            // if (IsEmpty()) return EmptyValue();
            string orderString = GetOrderString(order);
            if (orderString != "") orderString = " " + orderString;
            return ToStringValue(useSpace) + orderString;
        }

        public string GetExportString() {
            if (order == 0) return "";
            var str = "-" + GetOrderString(order);
            // if (type == AmountModifierType.Raw) str += "-Val";
            return str;
        }

        private string EmptyValue() => IsOneDefault() ? "1" : "0";
        public Amount GetRealAdjust() => adjustReal;
        public long GetExponent() => adjustReal.Exponent;
        public bool IsOneDefault() => type == AmountModifierType.MultiStacking || type == AmountModifierType.Exponent;

        public bool Equals(AmountModifier modifier) {
            return modifier.adjustReal == adjustReal &&
                modifier.order == order && 
                modifier.type == type;

        }
        
        /**
        * Statics
        */
        public static string GetOrderString(int orderN) {
            if (orderN <= -2) return "Base";
            if (orderN == -1) return "Base";
            if (orderN == 1) return "Total";
            if (orderN > 1) return "Total";
            return "";
        }

        /**
        * Adjust Amount
        */
        public static Amount AdjustWith(Amount baseValue, IEnumerable<AmountModifier> modifiers) {
            Amount value = baseValue;
            if (!modifiers.Any()) return baseValue;

            var sameOrderLists = CombineSameOrderLists(modifiers);
            foreach (var sameOrderList in sameOrderLists) {
                value = AdjustWithRaw(value, sameOrderList);
            }
            
            return value;
        }
        
        /**
        * Adjust AmountModifier
        */
        public static AmountModifier AdjustWith(AmountModifier baseModifier, IEnumerable<AmountModifier> modifiers) {
            AmountModifier modifier = baseModifier;
            if (!modifiers.Any()) return baseModifier;

            var sameOrderLists = CombineSameOrderLists(modifiers);
            foreach (var sameOrderList in sameOrderLists) {
                modifier = AdjustWithRaw(modifier, sameOrderList);
            }
            
            return modifier;
        }
        
        public static List<List<AmountModifier>> CombineSameOrderLists(IEnumerable<AmountModifier> modifiers) {
            List<List<AmountModifier>> sameOrderLists = new List<List<AmountModifier>>();
            List<AmountModifier> sorted = SortList(modifiers);
            List<AmountModifier> currentOrderList = new List<AmountModifier>();
            int currentOrder = sorted.Count > 0 ? sorted[0].order : 0;
            for (int i = 0; i < sorted.Count; i++) {
                AmountModifier current = sorted[i];
                if (current.order == currentOrder) {
                    currentOrderList.Add(current);
                    continue;
                }

                List<AmountModifier> reducedList = CombineSameOrderList(currentOrderList);
                sameOrderLists.Add(reducedList);

                currentOrderList = new List<AmountModifier>{current};
                currentOrder = current.order;
            }

            if (currentOrderList.Count > 0) {
                List<AmountModifier> reducedList = CombineSameOrderList(currentOrderList);
                sameOrderLists.Add(reducedList);
            }
            
            return sameOrderLists;
        }

        public static Amount AdjustWithRaw(Amount baseValue, IEnumerable<AmountModifier> modifiers) {
            return modifiers.Aggregate(baseValue, (current, amountModifier) => amountModifier.Adjust(current));
        }

        public static AmountModifier AdjustWithRaw(AmountModifier baseValue, IEnumerable<AmountModifier> modifiers) {
            return modifiers.Aggregate(baseValue, (current, amountModifier) => amountModifier.Adjust(current));
        }

        /**
        * Creators
        */
        public static AmountModifier empty = Raw(0);
        public static AmountModifier Raw(Amount value) => new AmountModifier(value, AmountModifierType.Raw);
        public static AmountModifier Diminishing(Amount value) => new AmountModifier(value, AmountModifierType.MultiDiminishing);
        public static AmountModifier Stacking(Amount value) => new AmountModifier(value + 1, AmountModifierType.MultiStacking);
        public static AmountModifier StackingRaw(Amount value) => new AmountModifier(value, AmountModifierType.MultiStacking);
        public static AmountModifier Reduction(Amount value) => new AmountModifier(value, AmountModifierType.Reduction);
        public static AmountModifier Exponent(Amount value) => new AmountModifier(value + 1, AmountModifierType.Exponent);
    }
}