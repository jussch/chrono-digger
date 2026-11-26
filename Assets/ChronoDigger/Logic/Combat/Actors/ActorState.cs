using System;
using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat {
    [Serializable]
    public class ActorState {
        public int Health;
        public int HealthRegenCycle;
        public int Mana;
        public int ManaRegenCycle;
        public int Block;
        public int Stamina;

        public List<StatusEffect> StatusEffects = new List<StatusEffect>();

        public ActorState(StatBlock statBlock) {
            Health = statBlock.Get(Stat.MaxHealth).ToInt();
            Mana = statBlock.Get(Stat.MaxMana).ToInt();
            HealthRegenCycle = statBlock.Get(Stat.HealthRegenTime).ToInt();
            ManaRegenCycle = statBlock.Get(Stat.ManaRegenTime).ToInt();
            // ...
        }
    }
}
