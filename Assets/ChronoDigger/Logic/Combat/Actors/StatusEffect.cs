using System;

namespace ChronoDigger.Combat {
    public class StatusEffect {
        public StatusEffectDefinition Definition { get; }
        // Saving this will probably need an id reference rather than the Actor itself.
        public Actor Source { get; }
        public int TimeLeft { get; private set; }
        public int TimeTotal { get; private set; }
        public int Stacks { get; private set; }

        public StatusEffect(StatusEffectDefinition definition, int stacks = 0, Actor source = null) {
            Definition = definition;
            Source = source;
            TimeTotal = definition.BaseTime;
            TimeLeft = definition.BaseTime;
            AddStacks(stacks);
        }

        /// <summary>Clamps at the definition's max stacks.</summary>
        public void AddStacks(int stacks) {
            Stacks = Math.Min(Stacks + stacks, Definition.MaxStacks);
        }

        public void Increment(int time) {
            TimeLeft -= time;
        }

        public void Reload() {
            TimeTotal = Definition.BaseTime;
            TimeLeft = Definition.BaseTime;
        }

        public void ApplyToStatTable(StatTable statTable) {
            foreach (var statMod in Definition.StatModifiers) {
                // TODO: These will need descriptors later.
                if (Definition.StacksMultiplyModifiers) {
                    statMod.Retrieve(statTable).Add(Definition, statMod.Modifier.MultiplyScalar(Stacks));
                } else {
                    statMod.Retrieve(statTable).Add(Definition, statMod.Modifier);
                }
            }
        }

        public void RemoveFromStatTable(StatTable statTable) {
            foreach (var statMod in Definition.StatModifiers) {
                statMod.Retrieve(statTable).Remove(Definition);
            }
        }

        public bool IsExpired() => TimeLeft <= 0;
    }
}
