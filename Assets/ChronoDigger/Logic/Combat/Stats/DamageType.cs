namespace ChronoDigger.Combat.Stats {
    /// <summary>A kind of damage, paired with the ordinary stat that resists it.</summary>
    // [CreateAssetMenu(menuName = "Combat/Damage Type")]
    public class DamageType /* : ScriptableObject */ {
        public string displayName;
        public StatDefinition resistanceStat;

        public override string ToString() => displayName;
    }
}
