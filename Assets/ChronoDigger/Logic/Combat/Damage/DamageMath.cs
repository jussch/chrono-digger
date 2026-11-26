using Marple.Core;

namespace ChronoDigger.Combat {
    public static class DamageMath {
        public const double DimishingValue = 10;
        public static Amount DiminishingReduction(Amount reduction) {
            return 1 - (reduction / (DimishingValue + reduction));
        }

        public static Amount ApplyResistance(Amount damage, Amount resistance) {
            return damage * DiminishingReduction(resistance);
        }
    }
}