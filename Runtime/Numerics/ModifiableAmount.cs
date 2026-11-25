using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
    [Serializable]
    public class ModifiableAmount : AmountModifierRecord {
        public double baseValue;

        [NonSerialized]
        private Amount calculatedValue;

        public string calculatedValueReadable => Utils.BeautifyNumber(GetValue(), Utils.BeautifySettings.WithSpace(false));

        private bool calculationDirty = true;

        // public static implicit operator double(ModifiableAmount record) => record.GetValue().ToDouble();
        public static implicit operator Amount(ModifiableAmount record) => record.GetValue();
        // public static implicit operator int(ModifiableAmount record) => record.AsInt();


        public static Amount operator +(ModifiableAmount left, Amount right) => left.GetValue() + right;
        public static Amount operator -(ModifiableAmount left, Amount right) => left.GetValue() - right;
        public static Amount operator *(ModifiableAmount left, Amount right) => left.GetValue() * right;
        public static Amount operator /(ModifiableAmount left, Amount right) => left.GetValue() / right;
        public static Amount operator +(Amount left, ModifiableAmount right) => left + right.GetValue();
        public static Amount operator -(Amount left, ModifiableAmount right) => left - right.GetValue();
        public static Amount operator *(Amount left, ModifiableAmount right) => left * right.GetValue();
        public static Amount operator /(Amount left, ModifiableAmount right) => left / right.GetValue();
        public static Amount operator +(ModifiableAmount left, ModifiableAmount right) => left.GetValue() + right.GetValue();
        public static Amount operator -(ModifiableAmount left, ModifiableAmount right) => left.GetValue() - right.GetValue();
        public static Amount operator *(ModifiableAmount left, ModifiableAmount right) => left.GetValue() * right.GetValue();
        public static Amount operator /(ModifiableAmount left, ModifiableAmount right) => left.GetValue() / right.GetValue();
        public static bool operator ==(ModifiableAmount left, Amount right) => left?.GetValue() == right;
        public static bool operator !=(ModifiableAmount left, Amount right) => left?.GetValue() != right;

        // The ==/!= above compare the current value against an Amount. Equality between
        // ModifiableAmounts stays by reference: they're mutable, and they're used as
        // AmountModifierKey sources, so value equality would make distinct instances collide.
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
        public static bool operator <=(ModifiableAmount left, Amount right) => left.GetValue() <= right;
        public static bool operator >=(ModifiableAmount left, Amount right) => left.GetValue() >= right;
        public static bool operator >(ModifiableAmount left, Amount right) => left.GetValue() > right;
        public static bool operator <(ModifiableAmount left, Amount right) => left.GetValue() < right;
        //
        // public static Amount operator +(ModifiableAmount left, double right) => left.GetValue() + right;
        // public static Amount operator -(ModifiableAmount left, double right) => left.GetValue() - right;
        // public static Amount operator *(ModifiableAmount left, double right) => left.GetValue() * right;
        // public static Amount operator /(ModifiableAmount left, double right) => left.GetValue() / right;
        // public static bool operator ==(ModifiableAmount left, double right) => left?.GetValue() == right;
        // public static bool operator !=(ModifiableAmount left, double right) => left?.GetValue() != right;
        // public static bool operator <=(ModifiableAmount left, double right) => left.GetValue() <= right;
        // public static bool operator >=(ModifiableAmount left, double right) => left.GetValue() >= right;

        public ModifiableAmount(double baseValueN) {
            baseValue = baseValueN;
            calculatedValue = baseValueN;
        }

        public Amount GetValue() {
            if (calculationDirty) Calculate();
            return calculatedValue;
        }

        public ModifiableAmount SetBase(Amount val) {
            baseValue = val.ToDouble();
            Calculate();
            return this;
        }

        public ModifiableAmount SetBaseCalc(Amount val) {
            Add(this, AmountModifier.Raw(val - baseValue).SetOrder(-2));
            return this;
        }

        protected override void MakeDirty() {
            base.MakeDirty();
            calculationDirty = true;
        }

        public void Calculate() {
            Amount newValue = Adjust(baseValue);
            // if ((newValue - calculatedValue).Abs() > 0.001d) UpdateObservable();
            if (!Utils.ApproxError(newValue, calculatedValue)) UpdateObservable();
            calculatedValue = newValue;
            calculationDirty = false;
        }

        public void Load(ModifiableAmount otherRecord) {
            modifiers = otherRecord.modifiers ?? new Dictionary<AmountModifierKey, Entry>();
            Calculate();
        }

        public AmountModifier CreateStackingMod() {
            return AmountModifier.Stacking((GetValue() - 100) / 100);
        }

        public Amount GetDiff(AmountModifierKey key, AmountModifier newMod) => GetDiff(baseValue, key, newMod);

        public bool IsChanged() => baseValue != calculatedValue;
        
        public int ToInt() => GetValue().ToInt();
        public Amount ToAmount() => GetValue();

        public Amount AsPercent() => GetValue().AsPercent();

        public string AsReadable(string suffix = "") {
            return Utils.BeautifyNumber(GetValue()) + suffix + " (" + AsReadableStringBonus(true, baseValue) + ")";
        }

        public string AsMiniReadablePercent(string suffix = "") {
            return Utils.BeautifyNumber(GetValue()) + "%" + suffix;
        }

        public string AsReadablePercent(string suffix = "") {
            return AsReadable("%" + suffix);
        }

        public new static ModifiableAmount empty = new ModifiableAmount(0);
    }
}
