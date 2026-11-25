using NUnit.Framework;
using ChronoDigger.Combat;

namespace ChronoDigger.Tests {
    public class CombatPlaceholderTests {
        [TestCase(10, 3, 7)]
        [TestCase(3, 10, 0)]
        public void Damage_IsAttackMinusDefense_NeverNegative(double atk, double def, double expected)
            => Assert.AreEqual(expected, CombatPlaceholder.Damage(atk, def));
    }
}
