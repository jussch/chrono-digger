using ChronoDigger.Combat;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class StatDerivationValidatorTests {
        [Test]
        public void FindCycle_AToBToA_ReturnsPath() {
            var a = TestStats.Create("A");
            var b = TestStats.Create("B");
            TestStats.Derive(a, b, AmountModifier.Raw(1));
            TestStats.Derive(b, a, AmountModifier.Raw(1));

            CollectionAssert.AreEqual(new[] { a, b, a }, StatDerivationValidator.FindCycle(a));
        }

        [Test]
        public void FindCycle_ChainWithoutCycle_ReturnsNull() {
            var a = TestStats.Create("A");
            var b = TestStats.Create("B");
            var c = TestStats.Create("C");
            TestStats.Derive(a, b, AmountModifier.Raw(1));
            TestStats.Derive(b, c, AmountModifier.Raw(1));
            TestStats.Derive(a, c, AmountModifier.Raw(1)); // diamond, not a cycle

            Assert.IsNull(StatDerivationValidator.FindCycle(a));
        }

        [Test]
        public void FindCycle_SelfReference_ReturnsPath() {
            var a = TestStats.Create("A");
            TestStats.Derive(a, a, AmountModifier.Raw(1));

            CollectionAssert.AreEqual(new[] { a, a }, StatDerivationValidator.FindCycle(a));
        }

        [Test]
        public void FindCycle_SampleData_HasNoCycles() {
            Assert.IsNull(StatDerivationValidator.FindCycle(SampleStatData.Wizardry));
        }
    }
}
