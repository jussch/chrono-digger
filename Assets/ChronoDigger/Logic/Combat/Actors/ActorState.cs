using System;
using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat {
    public enum ActorStateValues {
        Health,
        Mana,
        Block,
        Stamina,
    }

    [Serializable]
    public class ActorState {
        public int Health;
        public int Mana;
        public int Block;
        public int Stamina;
        public int HealthRegenCycle;
        public int ManaRegenCycle;

        public List<StatusEffect> StatusEffects = new List<StatusEffect>();

        public ActorState(StatBlock statBlock) {
            Health = statBlock.Get(Stat.MaxHealth).ToInt();
            Mana = statBlock.Get(Stat.MaxMana).ToInt();
            HealthRegenCycle = statBlock.Get(Stat.HealthRegenTime).ToInt();
            ManaRegenCycle = statBlock.Get(Stat.ManaRegenTime).ToInt();
            // ...
        }

        public int GetValue(ActorStateValues stateValue) {
            return stateValue switch {
                ActorStateValues.Health => Health,
                ActorStateValues.Mana => Mana,
                ActorStateValues.Block => Block,
                ActorStateValues.Stamina => Stamina,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public void SetValue(ActorStateValues stateValue, int value) {
            switch (stateValue) {
                case ActorStateValues.Health: Health = value; break;
                case ActorStateValues.Mana: Mana = value; break;
                case ActorStateValues.Block: Block = value; break;
                case ActorStateValues.Stamina: Stamina = value; break;
                default: throw new ArgumentOutOfRangeException();
            };
        }

        public void ChangeValue(ActorStateValues stateValue, int change) {
            SetValue(stateValue, GetValue(stateValue) + change);
        }
    }
}
