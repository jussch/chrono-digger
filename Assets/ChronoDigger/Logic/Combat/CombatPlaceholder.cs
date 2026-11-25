namespace ChronoDigger.Combat {
    public static class CombatPlaceholder {
        public static double Damage(double attack, double defense) => System.Math.Max(0, attack - defense);
    }
}
