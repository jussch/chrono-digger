using System;

namespace ChronoDigger.Combat {
    [Serializable]
    public struct SlotState {
        public int Level;

        public StatTable BuildStatTable() {
            StatTable table = new StatTable();
            table.Get(Stat.ActionLevel).SetBase(Level);
            return table; 
        }
    }
}