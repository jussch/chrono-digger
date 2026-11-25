namespace ChronoDigger.Combat.Stats {
    /// <summary>Short names for the stats engine code refers to directly: <c>Stat.MaxHealth</c>.</summary>
    public static class Stat {
        static CoreStats Core => CoreStats.Instance;

        public static StatDefinition MaxHealth => Core.MaxHealth;
        public static StatDefinition MaxMana   => Core.MaxMana;
        public static StatDefinition Defence   => Core.Defence;
        public static StatDefinition Wizardry  => Core.Wizardry;
        public static StatDefinition Warrior   => Core.Warrior;
    }
}
