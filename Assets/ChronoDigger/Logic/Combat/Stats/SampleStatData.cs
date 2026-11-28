using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat {
    /// <summary>
    /// Stand-in for the assets that will exist once this runs in Unity. The only place that
    /// constructs stat definitions, damage types and profiles, so replacing it with assets
    /// is a one-file change. Values are placeholders.
    /// </summary>
    public static class SampleStatData {
        public const double ManaPerWizardry = 5;

        public static readonly StatDefinition MaxHealth = new StatDefinition {
            DisplayName = "Max Health", DefaultValue = 100, Category = StatCategory.Combat,
        };
        public static readonly StatDefinition HealthRegenTime = new StatDefinition {
            DisplayName = "Health Regen Time", DefaultValue = 10, Category = StatCategory.Combat,
        };
        public static readonly StatDefinition HealthRegen = new StatDefinition {
            DisplayName = "Health Regen", DefaultValue = 1, Category = StatCategory.Combat,
        };

        public static readonly StatDefinition MaxMana = new StatDefinition {
            DisplayName = "Max Mana", DefaultValue = 20, Category = StatCategory.Combat,
        };
        public static readonly StatDefinition ManaRegenTime = new StatDefinition {
            DisplayName = "Mana Regen Time", DefaultValue = 4, Category = StatCategory.Combat,
        };
        public static readonly StatDefinition ManaRegen = new StatDefinition {
            DisplayName = "Mana Regen", DefaultValue = 2, Category = StatCategory.Combat,
        };

        public static readonly StatDefinition Defence = new StatDefinition {
            DisplayName = "Defence", DefaultValue = 0, Category = StatCategory.Combat,
        };

        public static readonly StatDefinition Wizardry = new StatDefinition {
            DisplayName = "Wizardry", DefaultValue = 0, Category = StatCategory.Class,
            Derivations = new List<StatModifier> {
                new StatModifier { Stat = MaxMana, Modifier = AmountModifier.Raw(ManaPerWizardry) },
            },
        };

        public static readonly StatDefinition Warrior = new StatDefinition {
            DisplayName = "Warrior", DefaultValue = 0, Category = StatCategory.Class,
        };

        public static readonly StatDefinition FireResistance = new StatDefinition {
            DisplayName = "Fire Resistance", DefaultValue = 0, Category = StatCategory.Resistance,
        };

        public static readonly StatDefinition ActionLevel = new StatDefinition {
            DisplayName = "ActionLevel", DefaultValue = 0, Category = StatCategory.Action,
        };

        public static readonly DamageType Fire = new DamageType {
            DisplayName = "Fire", ResistanceStat = FireResistance,
        };

        public static readonly StatProfile GoblinShaman = new StatProfile {
            DisplayName = "Goblin Shaman",
            Overrides = new List<StatEntry> {
                new StatEntry { Stat = MaxHealth, BaseValue = 30 },
                new StatEntry { Stat = Wizardry, BaseValue = 3 },
            },
        };
    }
}
