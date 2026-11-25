using ChronoDigger.Combat.Stats;

namespace ChronoDigger.Combat.Actors {
    public class Actor {
        public ActorState state;
        public StatBlock statBlock;

        public void LoadStatBlock(StatProfile profile) {
            statBlock = new StatBlock(profile);
        }

        public void Initiate() {
            state = new ActorState(statBlock);
        }

        public void Increment(int time) {
            IncrementStatusEffects(time);
            IncrementActorState(time);
        }

        /**
         * Control
         **/
        public void GainHealth(int health) {
            state.health = Math.Min(state.health + health, statBlock.Get(Stat.MaxHealth));
        }

        public void LoseHealth(int healthLoss) {
            state.health = Math.Min(state.health - healthLoss, 0);
        }

        public void GainMana(int mana) {
            state.mana = Math.Min(state.mana + mana, statBlock.Get(Stat.MaxMana));
        }

        public void LoseMana(int manaLoss) {
            state.mana = Math.Min(state.mana - manaLoss, 0);
        }

        /**
         * Self State Management
         */
        public void IncrementActorState(int time) {
            state.healthRegenCycle -= time;
            if (stat.healthRegenCycle <= 0) {
                GainHealth(statBlock.Get(Stat.HealthRegen));
                stat.healthRegenCycle += statBlock.Get(Stat.HealthRegenTime);
            }

            state.manaRegenCycle -= time;
            if (stat.manaRegenCycle <= 0) {
                GainMana(statBlock.Get(Stat.ManaRegen));
                stat.manaRegenCycle += statBlock.Get(Stat.ManaRegenTime);
            }
        }

        /**
         * Status Effects
         **/
        public void AddStatusEffect(StatusEffect statusEffect) {
            state.statusEffects.Add(statusEffect);
            statusEffect.ApplyToStatBlock(statBlock);
        }

        public void RemoveStatusEffect(StatusEffect statusEffect) {
            state.statusEffects.Remove(statusEffect);
            statusEffect.RemoveFromStatBlock(statBlock);
        }

        public void IncrementStatusEffects(int time) {
            for (int i = state.statusEffects.Count - 1; i >= 0; i--) {
                var statusEffect = stat.statusEffects[i];
                statusEffect.Increment(time);
                if (statusEffect.IsExpired()) RemoveStatusEffect(statusEffect);
            }
        }
    }
}