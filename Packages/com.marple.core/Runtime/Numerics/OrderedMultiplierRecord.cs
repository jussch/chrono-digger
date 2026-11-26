using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
    [Serializable]
    public class OrderedMultiplierRecord : AmountModifierRecord {
        protected List<AmountModifierRecord> Records = new List<AmountModifierRecord>();
        protected HashSet<int> AppliedOrders = new HashSet<int>();
        protected Dictionary<int, AmountModifier> OrderedMultipliers = new Dictionary<int, AmountModifier>();

        public void AddRecord(AmountModifierRecord record) {
            Records.Add(record);
            ApplyModifiersTo(record);
        }

        public void RemoveRecord(AmountModifierRecord record) {
            Records.Remove(record);
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
            Records = new List<AmountModifierRecord>();
            AppliedOrders = new HashSet<int>();
            OrderedMultipliers = new Dictionary<int, AmountModifier>();
        }

        private void RecomputeOrderedMultipliers() {
            CollectOrderedMultipliers();
            foreach (var record in Records) {
                ApplyModifiersTo(record);
            }
        }

        private void ApplyModifiersTo(AmountModifierRecord record) {
            foreach (var order in AppliedOrders) {
                record.Remove(this, order);
            }
            foreach (var entry in OrderedMultipliers) {
                record.Add(this, entry.Key, entry.Value);
            }
        }

        private void CollectOrderedMultipliers() {
            List<List<AmountModifier>> sameOrderLists = AmountModifier.CombineSameOrderLists(GetAllModifiers());
            OrderedMultipliers = new Dictionary<int, AmountModifier>();
            foreach (var sameOrderList in sameOrderLists) {
                if (sameOrderList.Count == 0) continue;
                int order = sameOrderList[0].Order;
                Amount mod = AmountModifier.AdjustWithRaw(100, sameOrderList).AsPercent();
                OrderedMultipliers[order] = AmountModifier.StackingRaw(mod).SetOrder(order);
                AppliedOrders.Add(order);
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
