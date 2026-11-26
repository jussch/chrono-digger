using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
    [Serializable]
    public class AmountModifierRecord : IPassiveObservable {
        /// <summary>
        /// What a record stores per key: the modifier plus optional display info.
        /// A bare <see cref="AmountModifier"/> converts to an Entry implicitly, and display
        /// info is attached fluently: <c>AmountModifier.Raw(5).With(describable).With(TextState.insufficient)</c>.
        /// </summary>
        [Serializable]
        public readonly struct Entry {
            public readonly AmountModifier Modifier;
            public readonly IDescribable Describable;
            public readonly TextState State;

            public Entry(AmountModifier modifier, IDescribable describable = null, TextState state = default) {
                Modifier = modifier;
                Describable = describable;
                State = state;
            }

            public static implicit operator Entry(AmountModifier modifier) => new Entry(modifier);

            public Entry WithModifier(AmountModifier modifier) => new Entry(modifier, Describable, State);
            public Entry WithDescribable(IDescribable describable) => new Entry(Modifier, describable, State);
            public Entry WithState(TextState state) => new Entry(Modifier, Describable, state);

            public Entry With(IDescribable describable) => WithDescribable(describable);
            public Entry With(TextState state) => WithState(state);

            public override string ToString() {
                string text = Describable == null ? Modifier.ToString() : $"{Modifier} ({Describable.DisplayName})";
                return State.HasColor() ? $"{text} [{State}]" : text;
            }
        }

        public string ReadableString => AsReadableStringBonus();

        public Dictionary<AmountModifierKey, Entry> Modifiers;

        private int _observedId;
        protected bool PauseDirty;
        protected bool UpdateObservers;

        public AmountModifierRecord() {
            Modifiers = new Dictionary<AmountModifierKey, Entry>();
        }

        public AmountModifierRecord(AmountModifierRecord modifierRecord) {
            Modifiers = new Dictionary<AmountModifierKey, Entry>(modifierRecord.Modifiers);
        }

        public AmountModifierRecord Clone() => new AmountModifierRecord(this);

        /**
         * Additions
         */
        // Each overload takes an Entry; pass a bare AmountModifier or build one with .With(...).
        public void Add(object source, Entry entry) => Add(new AmountModifierKey(source), entry);
        public void Add(object source, int index, Entry entry) => Add(new AmountModifierKey(source, index), entry);

        /// <summary>The one place entries are written. Subclasses override this to react to additions.</summary>
        public virtual void Add(AmountModifierKey key, Entry entry) {
            if (entry.Modifier.IsEmpty()) {
                Remove(key);
                return;
            }
            Modifiers[key] = entry;
            MakeDirty();
        }

        /// <summary>Copies every entry from another record, keeping its keys.</summary>
        public void Add(AmountModifierRecord modifierRecord) {
            foreach (var entry in modifierRecord.Modifiers) {
                Add(entry.Key, entry.Value);
            }
        }

        public void Remove(object source, int index = 0) => Remove(new AmountModifierKey(source, index));
        public virtual void Remove(AmountModifierKey key) {
            Modifiers.Remove(key);
            MakeDirty();
        }

        /// <summary>Removes every modifier whose key also exists in the other record.</summary>
        public void Remove(AmountModifierRecord modifierRecord) {
            foreach (var key in modifierRecord.Modifiers.Keys) {
                Remove(key);
            }
        }

        public void Toggle(AmountModifierKey key, Entry entry, bool modeAdd) {
            if (modeAdd) Add(key, entry);
            else Remove(key);
        }

        public AmountModifierRecord Combine(ModifiableAmount modifiableAmount) {
            Add(new AmountModifierKey(modifiableAmount), AmountModifier.Raw(modifiableAmount.BaseValue).SetOrder(-2));
            Add(modifiableAmount);
            return this;
        }

        public AmountModifierRecord Combine(AmountModifierRecord modifierRecord) {
            Add(modifierRecord);
            return this;
        }

        public bool Has(object source, int index = 0) => Has(new AmountModifierKey(source, index));
        public bool Has(AmountModifierKey key) => Modifiers.ContainsKey(key);

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

        public IEnumerable<AmountModifier> GetAllModifiers() => Modifiers.Values.Select(entry => entry.Modifier);

        public IEnumerable<AmountModifier> GetAllModifiersSorted() => AmountModifier.SortListType(GetAllModifiers());

        public void PauseDirtying(bool pauseDirtyN) {
            PauseDirty = pauseDirtyN;
        }

        protected virtual void MakeDirty() {
            if (PauseDirty) return;
            UpdateObservers = true;
        }

        public bool IsEmpty() => Modifiers.Count == 0;

        public virtual void Clear() {
            Modifiers = new Dictionary<AmountModifierKey, Entry>();
            MakeDirty();
        }

        public void Load(AmountModifierRecord otherRecord) {
            Modifiers = otherRecord.Modifiers ?? new Dictionary<AmountModifierKey, Entry>();
            MakeDirty();
        }

        public int GetObservableId() {
            if (UpdateObservers) UpdateObservable();
            return _observedId;
        }

        protected void UpdateObservable() {
            _observedId += 1;
            UpdateObservers = false;
        }

        public string Print() {
            return string.Join(", ", Modifiers.Select(entry => $"[{entry.Key}] {entry.Value}"));
        }

        public string AsReadableString(bool useSpace = false) {
            List<AmountModifier> printList = GetAllModifiers().ToList();
            List<AmountModifier> rawMods = printList.FindAll(mod => mod.Type == AmountModifier.AmountModifierType.Raw);
            List<AmountModifier> multMods = printList.FindAll(mod => mod.Type != AmountModifier.AmountModifierType.Raw);
            string rawModNum = Utils.BeautifyNumberSigned(AmountModifier.AdjustWith(0, rawMods), Utils.BeautifySettings.WithSpace(useSpace));
            string multModNum = Utils.BeautifyNumber(AmountModifier.AdjustWith(1, multMods), Utils.BeautifySettings.WithSpace(useSpace));
            return "Mods: " + Modifiers.Count + " at [" + rawModNum + ", x" + multModNum + "]";
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

        public static AmountModifierRecord Empty = new AmountModifierRecord();
    }

    /// <summary>Starts an <see cref="AmountModifierRecord.Entry"/> chain from a modifier.</summary>
    public static class AmountModifierEntryExtensions {
        public static AmountModifierRecord.Entry With(this AmountModifier modifier, IDescribable describable) =>
            new AmountModifierRecord.Entry(modifier, describable);

        public static AmountModifierRecord.Entry With(this AmountModifier modifier, TextState state) =>
            new AmountModifierRecord.Entry(modifier, state: state);
    }
}
