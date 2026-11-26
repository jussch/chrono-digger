using System;
using Marple.Core;

namespace ChronoDigger.Combat {
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
            if (health <= 0) return;
            state.health = Math.Min(state.health + health, statBlock.Get(Stat.MaxHealth).ToInt());
        }

        public void LoseHealth(int healthLoss) {
            if (healthLoss <= 0) return;
            state.health = Math.Max(state.health - healthLoss, 0);
        }

        public void GainMana(int mana) {
            if (mana <= 0) return;
            state.mana = Math.Min(state.mana + mana, statBlock.Get(Stat.MaxMana).ToInt());
        }

        public void LoseMana(int manaLoss) {
            if (manaLoss <= 0) return;
            state.mana = Math.Max(state.mana - manaLoss, 0);
        }

        public void GainBlock(int block) {
            if (block <= 0) return;
            state.block += block;
        }

        public void LoseBlock(int blockLoss) {
            if (blockLoss <= 0) return;
            state.block = Math.Max(state.block - blockLoss, 0);
        }

        public void TakeDamage(DamageInfo damageInfo) {
            Amount damage = ProcessIncomingDamage(damageInfo.value, damageInfo);
            ApplyDamage(damage.ToInt(), damageInfo);
        }

        private Amount ProcessIncomingDamage(Amount damage, DamageInfo damageInfo) {
            if (!damageInfo.HasFlags(DamageFlags.IgnoreResistances) && damageInfo.type != null) {
                // Process resistance math
            }

            return damage;
        }

        // Takes final processed damage
        private void ApplyDamage(int damage, DamageInfo damageInfo) {
            if (state.block > 0 && !damageInfo.HasFlags(DamageFlags.IgnoreBlock)) {
                if (state.block >= damage) {
                    LoseBlock(damage);
                    damage = 0;
                } else {
                    damage -= state.block;
                    LoseBlock(state.block);
                }
            }

            LoseHealth(damage);
        }

        /**
         * Self State Management
         */
        public void IncrementActorState(int time) {
            state.healthRegenCycle -= time;
            if (state.healthRegenCycle <= 0) {
                GainHealth(statBlock.Get(Stat.HealthRegen).ToInt());
                state.healthRegenCycle += statBlock.Get(Stat.HealthRegenTime).ToInt();
            }

            state.manaRegenCycle -= time;
            if (state.manaRegenCycle <= 0) {
                GainMana(statBlock.Get(Stat.ManaRegen).ToInt());
                state.manaRegenCycle += statBlock.Get(Stat.ManaRegenTime).ToInt();
            }
        }

        /**
         * Status Effects
         **/
        public void AddStatusEffect(StatusEffect statusEffect) {
            var existing = state.statusEffects.Find(se => se.definition == statusEffect.definition);
            if (existing != null) {
                existing.AddStacks(statusEffect.stacks);
                existing.Reload();
                existing.ApplyToStatBlock(statBlock);
            } else {
                state.statusEffects.Add(statusEffect);
                statusEffect.ApplyToStatBlock(statBlock);
            }
        }

        public void RemoveStatusEffect(StatusEffect statusEffect) {
            state.statusEffects.Remove(statusEffect);
            statusEffect.RemoveFromStatBlock(statBlock);
        }

        public void IncrementStatusEffects(int time) {
            for (int i = state.statusEffects.Count - 1; i >= 0; i--) {
                var statusEffect = state.statusEffects[i];
                statusEffect.Increment(time);
                if (statusEffect.IsExpired()) RemoveStatusEffect(statusEffect);
            }
        }
    }
}