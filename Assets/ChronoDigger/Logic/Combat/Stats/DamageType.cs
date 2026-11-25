namespace ChronoDigger.Combat.Stats {
    // [CreateAssetMenu(menuName = "Combat/Damage Type")]
    public class DamageType /* : ScriptableObject */ {
        public string displayName;
        public StatDefinition resistanceStat;

        public override string ToString() => displayName;
    }
}
