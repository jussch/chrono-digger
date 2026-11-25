using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
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
                    entry.record.Add(pair.Key, pair.Value.WithModifier(entry.GetMod(pair.Value.Modifier)));
                }
            }

            return entry;
        }

        public void RemoveRecord(AmountModifierRecord record) {
            var entry = entries.Find(ent => ent.record == record);
            if (entry == null) return;
            entries.Remove(entry);
        }

        public override void Add(AmountModifierKey key, Entry modifierEntry) {
            base.Add(key, modifierEntry);
            foreach (var entry in entries) {
                if (!entry.condition()) continue;
                entry.wasActive = true;
                entry.record.Add(key, modifierEntry.WithModifier(entry.GetMod(modifierEntry.Modifier)));
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
                        entry.record.Add(pair.Key, pair.Value.WithModifier(entry.GetMod(pair.Value.Modifier)));
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
}
