using System;
using System.Collections.Generic;
using ChronoDigger.Combat.Stats;
using Marple.Core;

namespace ChronoDigger.Combat.Actors {
    [Serializable]
    public class ActorState {
        public int health;
        public int healthRegenCycle;
        public int mana;
        public int manaRegenCycle;
        public int block;
        public int stamina;

        public List<StatusEffect> statusEffects = new List<StatusEffect>();

        public ActorState(StatBlock statBlock) {
            health = statBlock.Get(Stat.MaxHealth).ToInt();
            mana = statBlock.Get(Stat.MaxMana).ToInt();
            healthRegenCycle = statBlock.Get(Stat.HealthRegenTime).ToInt();
            manaRegenCycle = statBlock.Get(Stat.ManaRegenTime).ToInt();
            // ...
        }
    }
}
