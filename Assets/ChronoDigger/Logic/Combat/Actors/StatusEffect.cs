using System;
using ChronoDigger.Combat.Stats;

namespace ChronoDigger.Combat.Actors {
    [Serializable]
    public class StatusEffect {
        public int timeLeft;
        public int timeTotal;
        public int stacks;

        // For this to be serializable, these probably have to be id references later.
        public Actor source;
        public StatusEffectDefinition definition;

        public StatusEffect(StatusEffectDefinition definitionN, int stacksN = 0) {
            definition = definitionN;
            timeTotal = definition.baseTime;
            timeLeft = definition.baseTime;
            AddStacks(stacksN);
        }

        public void AddStacks(int stacksN) {
            stacks = Math.Min(stacks + stacksN, definition.maxStacks);
        }

        public void Increment(int time) {
            timeLeft -= time;
        }

        public void Reload() {
            timeTotal = definition.baseTime;
            timeLeft = definition.baseTime;
        }

        public void ApplyToStatBlock(StatBlock statBlock) {
            foreach (var statMod in definition.statModifiers) {
                // TODO: These will need descriptors later.
                if (definition.stacksMultiplyModifiers) {
                    statMod.Retrieve(statBlock).Add(definition, statMod.modifier.MultiplyScalar(stacks));
                } else {
                    statMod.Retrieve(statBlock).Add(definition, statMod.modifier);
                }
            }
        }

        public void RemoveFromStatBlock(StatBlock statBlock) {
            foreach (var statMod in definition.statModifiers) {
                statMod.Retrieve(statBlock).Remove(definition);
            }
        }

        public bool IsExpired() => timeLeft <= 0;
    }
}