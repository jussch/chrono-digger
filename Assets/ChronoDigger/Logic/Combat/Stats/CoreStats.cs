namespace ChronoDigger.Combat.Stats {
    /// <summary>
    /// Registry of the few stats engine code names directly. Everything else is reached
    /// through data (DamageType.resistanceStat, spell fields, equipment).
    /// </summary>
    // [CreateAssetMenu(menuName = "Combat/Core Stats")]
    public class CoreStats /* : ScriptableObject */ {
        public StatDefinition MaxHealth, HealthRegenTime, HealthRegen, 
                            MaxMana, ManaRegenTime, ManaRegen, 
                            Defence, Wizardry, Warrior;

        private static CoreStats _instance;

        public static CoreStats Instance {
            get {
                // Unity version: _instance = Resources.Load<CoreStats>("CoreStats");
                // Explicit null check, not ??=, because Unity overloads == on UnityEngine.Object.
                if (_instance == null) _instance = CreateDefault();
                return _instance;
            }
        }

        /// <summary>Pass null to go back to the default.</summary>
        public static void SetInstance(CoreStats instance) => _instance = instance;

        // Temporary, Unity-free. Replaced by the Resources asset once this moves into Unity.
        private static CoreStats CreateDefault() => new CoreStats {
            MaxHealth = SampleStatData.MaxHealth,
            HealthRegenTime = SampleStatData.HealthRegenTime,
            HealthRegen = SampleStatData.HealthRegen,
            MaxMana = SampleStatData.MaxMana,
            ManaRegenTime = SampleStatData.ManaRegenTime,
            ManaRegen = SampleStatData.ManaRegen,
            Defence = SampleStatData.Defence,
            Wizardry = SampleStatData.Wizardry,
            Warrior = SampleStatData.Warrior,
        };
    }
}
