using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
    [Serializable]
    public class MergingModifierRecord : AmountModifierRecord {
        protected List<MergeEntry> Entries = new List<MergeEntry>();

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
            Entries.Add(entry);

            if (entry.Condition()) {
                foreach (var pair in Modifiers) {
                    entry.Record.Add(pair.Key, pair.Value.WithModifier(entry.GetMod(pair.Value.Modifier)));
                }
            }

            return entry;
        }

        public void RemoveRecord(AmountModifierRecord record) {
            var entry = Entries.Find(ent => ent.Record == record);
            if (entry == null) return;
            Entries.Remove(entry);
        }

        public override void Add(AmountModifierKey key, Entry modifierEntry) {
            base.Add(key, modifierEntry);
            foreach (var entry in Entries) {
                if (!entry.Condition()) continue;
                entry.WasActive = true;
                entry.Record.Add(key, modifierEntry.WithModifier(entry.GetMod(modifierEntry.Modifier)));
            }
        }

        public override void Remove(AmountModifierKey key) {
            base.Remove(key);
            foreach (var entry in Entries) {
                if (!entry.Condition()) continue;
                entry.WasActive = true;
                entry.Record.Remove(key);
            }
        }

        public void RecheckMods() {
            foreach (var entry in Entries) {
                if (!entry.WasActive && entry.Condition()) {
                    entry.WasActive = true;
                    foreach (var pair in Modifiers) {
                        entry.Record.Add(pair.Key, pair.Value.WithModifier(entry.GetMod(pair.Value.Modifier)));
                    }
                }
            }
        }

        public override void Clear() {
            base.Clear();
            Entries = new List<MergeEntry>();
        }

        public Amount GetDiff(AmountModifierKey key, AmountModifier newMod) => GetDiff(0, key, newMod);

        /**
         * Subclass
         */
        public class MergeEntry {
            public readonly AmountModifierRecord Record;
            public readonly Func<Amount> Mod;
            public readonly Func<Amount> ExpMod;
            public readonly Func<bool> Condition;
            public readonly Func<int> OrderAdjust;
            public bool WasActive;

            public MergeEntry(AmountModifierRecord record, Func<Amount> mod, Func<Amount> expMod, Func<bool> condition, Func<int> orderAdjust) {
                this.Record = record;
                this.Mod = mod;
                this.ExpMod = expMod;
                this.Condition = condition;
                this.OrderAdjust = orderAdjust;
                WasActive = this.Condition();
            }

            // public AmountModifier GetMod(AmountModifier inMod) => inMod.MultiplyScalar(mod);
            public AmountModifier GetMod(AmountModifier inMod) => inMod.Type == AmountModifier.AmountModifierType.MultiStacking || inMod.Order > 0
                ? inMod.PowerScalar(ExpMod()).AdjustOrder(OrderAdjust())
                : inMod.PowerScalar(ExpMod()).MultiplyScalar(Mod()).AdjustOrder(OrderAdjust());

            public string Print() => $"mod: {Mod()}, expMod: {ExpMod()}, condition: {Condition()}, orderAdjust: {OrderAdjust()}";
        }
    }
}
