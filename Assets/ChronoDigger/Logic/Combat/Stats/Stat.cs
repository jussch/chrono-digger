namespace ChronoDigger.Combat {
    public static class Stat {
        private static CoreStats Core => CoreStats.Instance;

        public static StatDefinition MaxHealth => Core.MaxHealth;
        public static StatDefinition HealthRegenTime => Core.HealthRegenTime;
        public static StatDefinition HealthRegen => Core.HealthRegen;
        public static StatDefinition MaxMana   => Core.MaxMana;
        public static StatDefinition ManaRegenTime   => Core.ManaRegenTime;
        public static StatDefinition ManaRegen   => Core.ManaRegen;
        public static StatDefinition Defence   => Core.Defence;
        public static StatDefinition Wizardry  => Core.Wizardry;
        public static StatDefinition Warrior   => Core.Warrior;
    }
}
