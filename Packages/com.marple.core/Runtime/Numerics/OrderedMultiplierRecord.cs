using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
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

        public override void Add(AmountModifierKey key, Entry entry) {
            base.Add(key, entry);
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
