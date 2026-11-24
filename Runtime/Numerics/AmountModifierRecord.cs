using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
    [Serializable]
    public class AmountModifierRecord {
        public string readableString => AsReadableStringBonus();

        public Dictionary<AmountModifierKey, AmountModifier> modifiers;

        private int observedId;
        protected bool pauseDirty;
        protected bool updateObservers;

        public AmountModifierRecord() {
            modifiers = new Dictionary<AmountModifierKey, AmountModifier>();
        }

        public AmountModifierRecord(AmountModifierRecord modifierRecord) {
            modifiers = new Dictionary<AmountModifierKey, AmountModifier>(modifierRecord.modifiers);
        }

        public AmountModifierRecord Clone() => new AmountModifierRecord(this);

        /**
         * Additions
         */
        public void Add(object source, AmountModifier modifier) => Add(new AmountModifierKey(source), modifier);
        public void Add(object source, int index, AmountModifier modifier) => Add(new AmountModifierKey(source, index), modifier);
        public virtual void Add(AmountModifierKey key, AmountModifier modifier) {
            if (modifier.IsEmpty()) {
                Remove(key);
                return;
            }
            modifiers[key] = modifier;
            MakeDirty();
        }

        /// <summary>Copies every modifier from another record, keeping its keys.</summary>
        public void Add(AmountModifierRecord modifierRecord) {
            foreach (var entry in modifierRecord.modifiers) {
                Add(entry.Key, entry.Value);
            }
        }

        public void Remove(object source, int index = 0) => Remove(new AmountModifierKey(source, index));
        public virtual void Remove(AmountModifierKey key) {
            modifiers.Remove(key);
            MakeDirty();
        }

        /// <summary>Removes every modifier whose key also exists in the other record.</summary>
        public void Remove(AmountModifierRecord modifierRecord) {
            foreach (var key in modifierRecord.modifiers.Keys) {
                Remove(key);
            }
        }

        public void Toggle(AmountModifierKey key, AmountModifier modifier, bool modeAdd) {
            if (modeAdd) Add(key, modifier);
            else Remove(key);
        }

        public AmountModifierRecord Combine(ModifiableAmount modifiableAmount) {
            Add(new AmountModifierKey(modifiableAmount), AmountModifier.Raw(modifiableAmount.baseValue).SetOrder(-2));
            Add(modifiableAmount);
            return this;
        }

        public AmountModifierRecord Combine(AmountModifierRecord modifierRecord) {
            Add(modifierRecord);
            return this;
        }

        public bool Has(object source, int index = 0) => Has(new AmountModifierKey(source, index));
        public bool Has(AmountModifierKey key) => modifiers.ContainsKey(key);

        /// <summary>The value change if <paramref name="modifier"/> were added at <paramref name="key"/>.</summary>
        protected Amount GetDiff(Amount baseValue, AmountModifierKey key, AmountModifier modifier) {
            var newRecord = Clone();
            newRecord.Add(key, modifier);
            return newRecord.Adjust(baseValue) - Adjust(baseValue);
        }

        /**
         * Value Methods
         */
        public Amount Adjust(Amount value) {
            if (IsEmpty()) return value;
            return AmountModifier.AdjustWith(value, GetAllModifiers());
        }

        public AmountModifier Adjust(AmountModifier modifier) {
            if (IsEmpty()) return modifier;
            return AmountModifier.AdjustWith(modifier, GetAllModifiers());
        }

        public IEnumerable<AmountModifier> GetAllModifiers() => modifiers.Values;

        public IEnumerable<AmountModifier> GetAllModifiersSorted() => AmountModifier.SortListType(modifiers.Values);

        public void PauseDirtying(bool pauseDirtyN) {
            pauseDirty = pauseDirtyN;
        }

        protected virtual void MakeDirty() {
            if (pauseDirty) return;
            updateObservers = true;
        }

        public bool IsEmpty() => modifiers.Count == 0;

        public virtual void Clear() {
            modifiers = new Dictionary<AmountModifierKey, AmountModifier>();
            MakeDirty();
        }

        public void Load(AmountModifierRecord otherRecord) {
            modifiers = otherRecord.modifiers ?? new Dictionary<AmountModifierKey, AmountModifier>();
            MakeDirty();
        }

        public int GetObservableId() {
            if (updateObservers) UpdateObservable();
            return observedId;
        }

        protected void UpdateObservable() {
            observedId += 1;
            updateObservers = false;
        }

        public string Print() {
            return string.Join(", ", modifiers.Select(entry => $"[{entry.Key}] {entry.Value}"));
        }

        public string AsReadableString(bool useSpace = false) {
            List<AmountModifier> printList = GetAllModifiers().ToList();
            List<AmountModifier> rawMods = printList.FindAll(mod => mod.Type == AmountModifier.AmountModifierType.Raw);
            List<AmountModifier> multMods = printList.FindAll(mod => mod.Type != AmountModifier.AmountModifierType.Raw);
            string rawModNum = Utils.BeautifyNumberSigned(AmountModifier.AdjustWith(0, rawMods), Utils.BeautifySettings.WithSpace(useSpace));
            string multModNum = Utils.BeautifyNumber(AmountModifier.AdjustWith(1, multMods), Utils.BeautifySettings.WithSpace(useSpace));
            return "Mods: " + modifiers.Count + " at [" + rawModNum + ", x" + multModNum + "]";
        }

        public string AsReadableStringBonus(bool useSpace = false, double rawBonusBase = 0) {
            List<AmountModifier> printList = GetAllModifiers().ToList();
            List<AmountModifier> rawMods = printList.FindAll(mod => mod.Type == AmountModifier.AmountModifierType.Raw);
            List<AmountModifier> multMods = printList.FindAll(mod => mod.Type != AmountModifier.AmountModifierType.Raw);
            Amount rawBonus = AmountModifier.AdjustWith(0, rawMods) + rawBonusBase;
            string multModNum = Utils.BeautifyNumber(AmountModifier.AdjustWith(1, multMods), Utils.BeautifySettings.WithSpace(useSpace));
            if (rawMods.Count == 0 && rawBonus == 0) return "x" + multModNum;

            string rawModNum = Utils.BeautifyNumberSigned(rawBonus, Utils.BeautifySettings.WithSpace(useSpace));
            return rawModNum + ", x" + multModNum;
        }

        public string PrintMods() {
            return string.Join(", ", GetAllModifiers());
        }

        public static AmountModifierRecord empty = new AmountModifierRecord();
    }

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
            modifiers = otherRecord.modifiers ?? new Dictionary<AmountModifierKey, AmountModifier>();
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

    [Serializable]
    public class MergingModifierRecord : AmountModifierRecord {
        protected List<MergeEntry> entries = new List<MergeEntry>();

        private static int Return0() => 0;
        private static Amount Return1() => 1;
        private static bool ReturnTrue() => true;

        public MergeEntry AddRecord(AmountModifierRecord record) => AddRecord(record, Return1, Return1, ReturnTrue);
        public MergeEntry AddRecord(AmountModifierRecord record, Func<Amount> recordMod) => AddRecord(record, recordMod, Return1, ReturnTrue);
        public MergeEntry AddRecord(AmountModifierRecord record, Func<bool> condition) => AddRecord(record, Return1, Return1, condition);
        public MergeEntry AddRecord(AmountModifierRecord record, Func<Amount> recordMod, Func<bool> condition) => AddRecord(record, recordMod, Return1, condition);
        public MergeEntry AddRecord(AmountModifierRecord record, Func<Amount> recordMod, Func<Amount> expMod) => AddRecord(record, recordMod, expMod, ReturnTrue);
        public MergeEntry AddRecord(AmountModifierRecord record, Func<Amount> recordMod, Func<Amount> expMod, Func<bool> condition) => AddRecord(record, recordMod, expMod, condition, Return0);
        public MergeEntry AddRecord(AmountModifierRecord record, Func<Amount> recordMod, Func<Amount> expMod, Func<int> orderAdjust) => AddRecord(record, recordMod, expMod, ReturnTrue, orderAdjust);
        public MergeEntry AddRecord(AmountModifierRecord record, Func<Amount> recordMod, Func<Amount> expMod, Func<bool> condition, Func<int> orderAdjust) {
            var entry = new MergeEntry(record, recordMod, expMod, condition, orderAdjust);
            entries.Add(entry);

            if (entry.condition()) {
                foreach (var pair in modifiers) {
                    entry.record.Add(pair.Key, entry.GetMod(pair.Value));
                }
            }

            return entry;
        }

        public void RemoveRecord(AmountModifierRecord record) {
            var entry = entries.Find(ent => ent.record == record);
            if (entry == null) return;
            entries.Remove(entry);
        }

        public override void Add(AmountModifierKey key, AmountModifier modifier) {
            base.Add(key, modifier);
            foreach (var entry in entries) {
                if (!entry.condition()) continue;
                entry.wasActive = true;
                entry.record.Add(key, entry.GetMod(modifier));
            }
        }

        public override void Remove(AmountModifierKey key) {
            base.Remove(key);
            foreach (var entry in entries) {
                if (!entry.condition()) continue;
                entry.wasActive = true;
                entry.record.Remove(key);
            }
        }

        public void RecheckMods() {
            foreach (var entry in entries) {
                if (!entry.wasActive && entry.condition()) {
                    entry.wasActive = true;
                    foreach (var pair in modifiers) {
                        entry.record.Add(pair.Key, entry.GetMod(pair.Value));
                    }
                }
            }
        }

        public override void Clear() {
            base.Clear();
            entries = new List<MergeEntry>();
        }

        public Amount GetDiff(AmountModifierKey key, AmountModifier newMod) => GetDiff(0, key, newMod);

        /**
         * Subclass
         */
        public class MergeEntry {
            public readonly AmountModifierRecord record;
            public readonly Func<Amount> mod;
            public readonly Func<Amount> expMod;
            public readonly Func<bool> condition;
            public readonly Func<int> orderAdjust;
            public bool wasActive;

            public MergeEntry(AmountModifierRecord record, Func<Amount> mod, Func<Amount> expMod, Func<bool> condition, Func<int> orderAdjust) {
                this.record = record;
                this.mod = mod;
                this.expMod = expMod;
                this.condition = condition;
                this.orderAdjust = orderAdjust;
                wasActive = this.condition();
            }

            // public AmountModifier GetMod(AmountModifier inMod) => inMod.MultiplyScalar(mod);
            public AmountModifier GetMod(AmountModifier inMod) => inMod.Type == AmountModifier.AmountModifierType.MultiStacking || inMod.Order > 0
                ? inMod.PowerScalar(expMod()).AdjustOrder(orderAdjust())
                : inMod.PowerScalar(expMod()).MultiplyScalar(mod()).AdjustOrder(orderAdjust());

            public string Print() => $"mod: {mod()}, expMod: {expMod()}, condition: {condition()}, orderAdjust: {orderAdjust()}";
        }
    }

    [Serializable]
    public class OrderedMultiplierRecord : AmountModifierRecord {
        protected List<AmountModifierRecord> records = new List<AmountModifierRecord>();
        protected HashSet<int> appliedOrders = new HashSet<int>();
        protected Dictionary<int, AmountModifier> orderedMultipliers = new Dictionary<int, AmountModifier>();

        public void AddRecord(AmountModifierRecord record) {
            records.Add(record);
            ApplyModifiersTo(record);
        }

        public void RemoveRecord(AmountModifierRecord record) {
            records.Remove(record);
        }

        public override void Add(AmountModifierKey key, AmountModifier modifier) {
            base.Add(key, modifier);
            RecomputeOrderedMultipliers();
        }

        public override void Remove(AmountModifierKey key) {
            base.Remove(key);
            RecomputeOrderedMultipliers();
        }

        public override void Clear() {
            base.Clear();
            records = new List<AmountModifierRecord>();
            appliedOrders = new HashSet<int>();
            orderedMultipliers = new Dictionary<int, AmountModifier>();
        }

        private void RecomputeOrderedMultipliers() {
            CollectOrderedMultipliers();
            foreach (var record in records) {
                ApplyModifiersTo(record);
            }
        }

        private void ApplyModifiersTo(AmountModifierRecord record) {
            foreach (var order in appliedOrders) {
                record.Remove(this, order);
            }
            foreach (var entry in orderedMultipliers) {
                record.Add(this, entry.Key, entry.Value);
            }
        }

        private void CollectOrderedMultipliers() {
            List<List<AmountModifier>> sameOrderLists = AmountModifier.CombineSameOrderLists(GetAllModifiers());
            orderedMultipliers = new Dictionary<int, AmountModifier>();
            foreach (var sameOrderList in sameOrderLists) {
                if (sameOrderList.Count == 0) continue;
                int order = sameOrderList[0].Order;
                Amount mod = AmountModifier.AdjustWithRaw(100, sameOrderList).AsPercent();
                orderedMultipliers[order] = AmountModifier.StackingRaw(mod).SetOrder(order);
                appliedOrders.Add(order);
            }
        }

        /**
         * External
         */
        public string GetTotalPercent() {
            return $"{Utils.BeautifyNumber(Adjust(100))}%";
        }

        public string GetTotalMultiplier() {
            return $"x{Utils.BeautifyNumber(Adjust(100).AsPercent())}";
        }
    }
}
