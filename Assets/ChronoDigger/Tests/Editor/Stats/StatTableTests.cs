using System;
using ChronoDigger.Combat;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class StatTableTests {
        private readonly object _buff = new object();

        [Test]
        public void GetValue_NotOverridden_ReturnsDefault() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatTable(TestStats.Profile());

            Assert.AreEqual((Amount)7, block.GetValue(defence));
        }

        [Test]
        public void GetValue_Overridden_ReturnsProfileValue() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatTable(TestStats.Profile((defence, 12)));

            Assert.AreEqual((Amount)12, block.GetValue(defence));
        }

        [Test]
        public void Get_NotOverridden_SeededFromDefault() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatTable(TestStats.Profile());

            Assert.AreEqual(7d, block.Get(defence).BaseValue);
        }

        [Test]
        public void AddingModifierThroughGetAmount_ChangesGet() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatTable(TestStats.Profile());

            block.Get(defence).Add(_buff, AmountModifier.Raw(3));

            Assert.AreEqual((Amount)10, block.GetValue(defence));
        }

        [Test]
        public void Derivation_AppliedFromProfileAtStart() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana", 20);
            TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));

            var block = new StatTable(TestStats.Profile((wizardry, 3)));
            block.Calculate();

            Assert.AreEqual((Amount)35, block.GetValue(maxMana));
        }

        [Test]
        public void Derivation_UpdatesWhenSourceChanges() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana", 20);
            TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));
            var block = new StatTable(TestStats.Profile((wizardry, 3)));
            block.Calculate();
            Amount before = block.GetValue(maxMana);

            block.Get(wizardry).Add(_buff, AmountModifier.Raw(1));
            block.Calculate();

            Assert.AreEqual(before + 5, block.GetValue(maxMana));
        }

        [Test]
        public void Derivation_RemovingSourceModifier_RevertsTarget() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana", 20);
            TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));
            var block = new StatTable(TestStats.Profile((wizardry, 3)));

            block.Get(wizardry).Add(_buff, AmountModifier.Raw(1));
            block.Calculate();
            block.Get(wizardry).Remove(_buff);
            block.Calculate();

            Assert.AreEqual((Amount)35, block.GetValue(maxMana));
        }

        [Test]
        public void Derivation_ChainOfTwo_AppliesOneLevel() {
            var a = TestStats.Create("A");
            var b = TestStats.Create("B", 1);
            var c = TestStats.Create("C");
            TestStats.Derive(a, b, AmountModifier.Raw(2));
            TestStats.Derive(b, c, AmountModifier.Raw(10));
            var block = new StatTable(TestStats.Profile((a, 1)));
            block.Calculate();

            Assert.AreEqual((Amount)3, block.GetValue(b));
            Assert.AreEqual((Amount)10, block.GetValue(c));
        }

        [Test]
        public void Calculate_ProfileOrderSwapped_SameResult() {
            var a = TestStats.Create("A");
            var b = TestStats.Create("B");
            var c = TestStats.Create("C");
            TestStats.Derive(a, b, AmountModifier.Raw(2));
            TestStats.Derive(b, c, AmountModifier.Raw(10));
            var forward = new StatTable(TestStats.Profile((a, 1), (b, 1), (c, 0)));
            var backward = new StatTable(TestStats.Profile((c, 0), (b, 1), (a, 1)));

            forward.Calculate();
            backward.Calculate();

            Assert.AreEqual(forward.GetValue(b), backward.GetValue(b));
            Assert.AreEqual(forward.GetValue(c), backward.GetValue(c));
        }

        [Test]
        public void Calculate_CalledRepeatedly_ResultUnchanged() {
            var a = TestStats.Create("A");
            var b = TestStats.Create("B");
            var c = TestStats.Create("C");
            TestStats.Derive(a, b, AmountModifier.Raw(2));
            TestStats.Derive(b, c, AmountModifier.Raw(10));
            var block = new StatTable(TestStats.Profile((a, 1), (b, 1)));
            block.Calculate();
            Amount b1 = block.GetValue(b), c1 = block.GetValue(c);

            block.Calculate();
            block.Calculate();

            Assert.AreEqual(b1, block.GetValue(b));
            Assert.AreEqual(c1, block.GetValue(c));
        }

        [Test]
        public void Calculate_DerivationCycle_UsesValuesWithoutDerivations() {
            var a = TestStats.Create("A");
            var b = TestStats.Create("B");
            TestStats.Derive(a, b, AmountModifier.Raw(1));
            TestStats.Derive(b, a, AmountModifier.Raw(1));
            var block = new StatTable(TestStats.Profile((a, 2), (b, 3)));

            block.Calculate();
            block.Calculate();

            Assert.AreEqual((Amount)5, block.GetValue(a));
            Assert.AreEqual((Amount)5, block.GetValue(b));
        }

        [Test]
        public void Calculate_Fraction_MultiplierReadsSource() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana", 100);
            var derivation = TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));
            derivation.Fraction = 2;
            wizardry.Derivations[0] = derivation;
            var block = new StatTable(TestStats.Profile((wizardry, 7)));

            block.Calculate();

            Assert.AreEqual((Amount)115, block.GetValue(maxMana));
        }

        [Test]
        public void Derivation_PerPointUnchangedAfterManyUpdates() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana");
            var derivation = TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));
            AmountModifier original = derivation.Modifier;
            var block = new StatTable(TestStats.Profile((wizardry, 3)));

            for (int i = 0; i < 100; i++) {
                block.Get(wizardry).Add(_buff, AmountModifier.Raw(i));
                block.Calculate();
            }

            Assert.AreEqual(original, derivation.Modifier);
            Assert.AreEqual((Amount)((3 + 99) * 5), block.GetValue(maxMana));
        }

        [Test]
        public void Derivation_SharedAcrossTables_DoesNotLeak() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana");
            TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));
            var strong = new StatTable(TestStats.Profile((wizardry, 10)));
            var weak = new StatTable(TestStats.Profile((wizardry, 1)));
            strong.Calculate();
            weak.Calculate();

            Assert.AreEqual((Amount)50, strong.GetValue(maxMana));
            Assert.AreEqual((Amount)5, weak.GetValue(maxMana));
        }

        [Test]
        public void Constructor_NullStat_Throws() {
            Assert.Throws<ArgumentException>(() => new StatTable(TestStats.Profile((null, 1))));
        }

        [Test]
        public void Constructor_DuplicateStat_Throws() {
            var defence = TestStats.Create("Defence");
            Assert.Throws<ArgumentException>(() => new StatTable(TestStats.Profile((defence, 1), (defence, 2))));
        }

        [Test]
        public void Calculate_DerivationCycle_Terminates() {
            var a = TestStats.Create("A");
            var b = TestStats.Create("B");
            TestStats.Derive(a, b, AmountModifier.Raw(1));
            TestStats.Derive(b, a, AmountModifier.Raw(1));
            var block = new StatTable(TestStats.Profile((a, 1)));

            Assert.DoesNotThrow(() => block.Calculate());
        }

        [Test]
        public void Calculate_IdenticalDerivations_AppliesEach() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana", 20);
            TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));
            TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));
            var block = new StatTable(TestStats.Profile((wizardry, 3)));

            block.Calculate();

            Assert.AreEqual((Amount)50, block.GetValue(maxMana));
        }
    }
}
