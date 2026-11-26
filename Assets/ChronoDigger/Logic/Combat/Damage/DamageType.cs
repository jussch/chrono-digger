namespace ChronoDigger.Combat {
    // [CreateAssetMenu(menuName = "Combat/Damage Type")]
    public class DamageType /* : ScriptableObject */ {
        public string DisplayName;
        public StatDefinition ResistanceStat;

        public override string ToString() => DisplayName;
    }
}
