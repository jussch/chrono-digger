namespace ChronoDigger.Combat {
    /// <summary>
    /// Stand-in for action assets. The only place that constructs action definitions.
    /// Effects are empty until damage/heal effects exist.
    /// </summary>
    public static class SampleActionData {
        public static readonly ActionDefinition SwingSword = new ActionDefinition {
            DisplayName = "Swing Sword", Windup = 1, Cooldown = 0,
            Targeting = new SingleActorTargeting { Range = 1 },
        };

        public static readonly ActionDefinition Parry = new ActionDefinition {
            DisplayName = "Parry", Windup = 0, Cooldown = 2,
            Targeting = new SelfTargeting(),
        };

        public static readonly ActionDefinition CastFireball = new ActionDefinition {
            DisplayName = "Cast Fireball", Windup = 3, Cooldown = 2,
            Targeting = new SingleActorTargeting { Range = 3 },
        };

        public static readonly ActionDefinition DrinkHealingPotion = new ActionDefinition {
            DisplayName = "Drink Healing Potion", Windup = 0, Cooldown = 0,
            Targeting = new SelfTargeting(),
            Costs = { new ConsumeSourceCost { Amount = 1 } },
        };

        /// <summary>What a sword grants: Swing and Parry.</summary>
        public static readonly ActionDefinition[] SwordActions = { SwingSword, Parry };
    }
}
