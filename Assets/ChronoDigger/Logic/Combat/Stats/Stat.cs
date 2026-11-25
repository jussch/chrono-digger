namespace ChronoDigger.Combat.Stats {
    public static class Stat {
        private static CoreStats Core => CoreStats.Instance;

        public static StatDefinition MaxHealth => Core.MaxHealth;
        public static StatDefinition MaxMana   => Core.MaxMana;
        public static StatDefinition Defence   => Core.Defence;
        public static StatDefinition Wizardry  => Core.Wizardry;
        public static StatDefinition Warrior   => Core.Warrior;
    }
}
