using System;
using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat {
    public class Actor {
        public StatTable Stats { get; private set; }
        public GridPos Position { get; set; }
        public ActionSet Actions { get; } = new ActionSet();
        /// <summary>Null when idle. Set and cleared only by <see cref="ActionRules"/>.</summary>
        public Intent CurrentIntent { get; internal set; }

        private ActorState _state;

        public int Health => _state.Health;
        public int Mana => _state.Mana;
        public int Block => _state.Block;
        public IReadOnlyList<StatusEffect> StatusEffects => _state.StatusEffects;

        public void LoadStatTable(StatProfile profile) {
            Stats = new StatTable(profile);
        }

        public void Initiate() {
            _state = new ActorState(Stats);
        }

        public void Increment(int time) {
            IncrementStatusEffects(time);
            IncrementActorState(time);
        }

        /**
         * Control
         **/

        public int GetStateValue(ActorStateValues stateValue) => _state.GetValue(stateValue);
        public void SetStateValue(ActorStateValues stateValue, int value) => _state.SetValue(stateValue, value);
        public void ChangeStateValue(ActorStateValues stateValue, int value) => _state.ChangeValue(stateValue, value);

        public void GainHealth(int health) {
            if (health <= 0) return;
            _state.Health = Math.Min(_state.Health + health, Stats.GetValue(Stat.MaxHealth).ToInt());
        }

        public void LoseHealth(int health) {
            if (health <= 0) return;
            _state.Health = Math.Max(_state.Health - health, 0);
        }

        public void GainMana(int mana) {
            if (mana <= 0) return;
            _state.Mana = Math.Min(_state.Mana + mana, Stats.GetValue(Stat.MaxMana).ToInt());
        }

        public void LoseMana(int mana) {
            if (mana <= 0) return;
            _state.Mana = Math.Max(_state.Mana - mana, 0);
        }

        public void GainBlock(int block) {
            if (block <= 0) return;
            _state.Block += block;
        }

        public void LoseBlock(int block) {
            if (block <= 0) return;
            _state.Block = Math.Max(_state.Block - block, 0);
        }

        public void TakeDamage(DamageInfo damageInfo) {
            Amount damage = ProcessIncomingDamage(damageInfo.Value, damageInfo);
            ApplyDamage(damage.ToInt(), damageInfo);
        }

        private Amount ProcessIncomingDamage(Amount damage, DamageInfo damageInfo) {
            if (!damageInfo.HasFlags(DamageFlags.IgnoreResistances) && damageInfo.Type != null) {
                // Process resistance math
                damage = DamageMath.ApplyResistance(damage, Stats.GetValue(damageInfo.Type.ResistanceStat));
            }

            return damage;
        }

        // Takes final processed damage
        private void ApplyDamage(int damage, DamageInfo damageInfo) {
            if (_state.Block > 0 && !damageInfo.HasFlags(DamageFlags.IgnoreBlock)) {
                if (_state.Block >= damage) {
                    LoseBlock(damage);
                    damage = 0;
                } else {
                    damage -= _state.Block;
                    LoseBlock(_state.Block);
                }
            }

            LoseHealth(damage);
        }

        /**
         * Self State Management
         */
        public void IncrementActorState(int time) {
            _state.HealthRegenCycle -= time;
            if (_state.HealthRegenCycle <= 0) {
                GainHealth(Stats.GetValue(Stat.HealthRegen).ToInt());
                _state.HealthRegenCycle += Stats.GetValue(Stat.HealthRegenTime).ToInt();
            }

            _state.ManaRegenCycle -= time;
            if (_state.ManaRegenCycle <= 0) {
                GainMana(Stats.GetValue(Stat.ManaRegen).ToInt());
                _state.ManaRegenCycle += Stats.GetValue(Stat.ManaRegenTime).ToInt();
            }
        }

        /**
         * Status Effects
         **/
        public void AddStatusEffect(StatusEffect statusEffect) {
            var existing = _state.StatusEffects.Find(se => se.Definition == statusEffect.Definition);
            if (existing != null) {
                existing.AddStacks(statusEffect.Stacks);
                existing.Reload();
                existing.ApplyToStatTable(Stats);
            } else {
                _state.StatusEffects.Add(statusEffect);
                statusEffect.ApplyToStatTable(Stats);
            }
        }

        public void RemoveStatusEffect(StatusEffect statusEffect) {
            _state.StatusEffects.Remove(statusEffect);
            statusEffect.RemoveFromStatTable(Stats);
        }

        private void IncrementStatusEffects(int time) {
            for (int i = _state.StatusEffects.Count - 1; i >= 0; i--) {
                var statusEffect = _state.StatusEffects[i];
                statusEffect.Increment(time);
                if (statusEffect.IsExpired()) RemoveStatusEffect(statusEffect);
            }
        }
    }
}