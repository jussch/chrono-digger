using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat.Stats {
    /// <summary>
    /// Stand-in for the assets that will exist once this runs in Unity. The only place that
    /// constructs stat definitions, damage types and profiles, so replacing it with assets
    /// is a one-file change. Values are placeholders.
    /// </summary>
    public static class SampleStatData {
        public const double ManaPerWizardry = 5;

        public static readonly StatDefinition MaxHealth = new StatDefinition {
            displayName = "Max Health", defaultValue = 100, category = StatCategory.Combat,
        };

        public static readonly StatDefinition MaxMana = new StatDefinition {
            displayName = "Max Mana", defaultValue = 20, category = StatCategory.Combat,
        };

        public static readonly StatDefinition Defence = new StatDefinition {
            displayName = "Defence", defaultValue = 0, category = StatCategory.Combat,
        };

        public static readonly StatDefinition Wizardry = new StatDefinition {
            displayName = "Wizardry", defaultValue = 0, category = StatCategory.Class,
            derivations = new List<StatDerivation> {
                new StatDerivation { target = MaxMana, perPoint = AmountModifier.Raw(ManaPerWizardry) },
            },
        };

        public static readonly StatDefinition Warrior = new StatDefinition {
            displayName = "Warrior", defaultValue = 0, category = StatCategory.Class,
        };

        public static readonly StatDefinition FireResistance = new StatDefinition {
            displayName = "Fire Resistance", defaultValue = 0, category = StatCategory.Resistance,
        };

        public static readonly DamageType Fire = new DamageType {
            displayName = "Fire", resistanceStat = FireResistance,
        };

        public static readonly StatProfile GoblinShaman = new StatProfile {
            displayName = "Goblin Shaman",
            overrides = new List<StatEntry> {
                new StatEntry { stat = MaxHealth, baseValue = 30 },
                new StatEntry { stat = Wizardry, baseValue = 3 },
            },
        };
    }
}
