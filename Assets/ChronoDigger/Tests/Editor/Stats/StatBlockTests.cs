using System;
using ChronoDigger.Combat.Stats;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests.Stats {
    public class StatBlockTests {
        private readonly object buff = new object();

        [Test]
        public void Get_NotOverridden_ReturnsDefault() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatBlock(TestStats.Profile());

            Assert.AreEqual((Amount)7, block.Get(defence));
        }

        [Test]
        public void Get_Overridden_ReturnsProfileValue() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatBlock(TestStats.Profile((defence, 12)));

            Assert.AreEqual((Amount)12, block.Get(defence));
        }

        [Test]
        public void GetAmount_NotOverridden_SeededFromDefault() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatBlock(TestStats.Profile());

            Assert.AreEqual(7d, block.GetAmount(defence).baseValue);
        }

        [Test]
        public void AddingModifierThroughGetAmount_ChangesGet() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatBlock(TestStats.Profile());

            block.GetAmount(defence).Add(buff, AmountModifier.Raw(3));

            Assert.AreEqual((Amount)10, block.Get(defence));
        }

        [Test]
        public void Derivation_AppliedFromProfileAtStart() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana", 20);
            TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));

            var block = new StatBlock(TestStats.Profile((wizardry, 3)));
            block.Calculate();

            Assert.AreEqual((Amount)35, block.Get(maxMana));
        }

        [Test]
        public void Derivation_UpdatesWhenSourceChanges() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana", 20);
            TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));
            var block = new StatBlock(TestStats.Profile((wizardry, 3)));
            block.Calculate();
            Amount before = block.Get(maxMana);

            block.GetAmount(wizardry).Add(buff, AmountModifier.Raw(1));
            block.Calculate();

            Assert.AreEqual(before + 5, block.Get(maxMana));
        }

        [Test]
        public void Derivation_RemovingSourceModifier_RevertsTarget() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana", 20);
            TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));
            var block = new StatBlock(TestStats.Profile((wizardry, 3)));

            block.GetAmount(wizardry).Add(buff, AmountModifier.Raw(1));
            block.Calculate();
            block.GetAmount(wizardry).Remove(buff);
            block.Calculate();

            Assert.AreEqual((Amount)35, block.Get(maxMana));
        }

        [Test]
        public void Derivation_ChainOfTwo_Propagates() {
            var a = TestStats.Create("A");
            var b = TestStats.Create("B");
            var c = TestStats.Create("C");
            TestStats.Derive(a, b, AmountModifier.Raw(2));
            TestStats.Derive(b, c, AmountModifier.Raw(10));
            var block = new StatBlock(TestStats.Profile((a, 1)));
            block.Calculate();

            Assert.AreEqual((Amount)2, block.Get(b));
            Assert.AreEqual((Amount)20, block.Get(c));

            block.GetAmount(a).Add(buff, AmountModifier.Raw(1));
            block.Calculate();

            Assert.AreEqual((Amount)4, block.Get(b));
            Assert.AreEqual((Amount)40, block.Get(c));
        }

        [Test]
        public void Derivation_PerPointUnchangedAfterManyUpdates() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana");
            var derivation = TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));
            AmountModifier original = derivation.perPoint;
            var block = new StatBlock(TestStats.Profile((wizardry, 3)));

            for (int i = 0; i < 100; i++) {
                block.GetAmount(wizardry).Add(buff, AmountModifier.Raw(i));
                block.Calculate();
            }

            Assert.AreEqual(original, derivation.perPoint);
            Assert.AreEqual((Amount)((3 + 99) * 5), block.Get(maxMana));
        }

        [Test]
        public void Derivation_SharedAcrossBlocks_DoesNotLeak() {
            var wizardry = TestStats.Create("Wizardry");
            var maxMana = TestStats.Create("Max Mana");
            TestStats.Derive(wizardry, maxMana, AmountModifier.Raw(5));
            var strong = new StatBlock(TestStats.Profile((wizardry, 10)));
            var weak = new StatBlock(TestStats.Profile((wizardry, 1)));
            strong.Calculate();
            weak.Calculate();

            Assert.AreEqual((Amount)50, strong.Get(maxMana));
            Assert.AreEqual((Amount)5, weak.Get(maxMana));
        }

        [Test]
        public void Constructor_NullStat_Throws() {
            Assert.Throws<ArgumentException>(() => new StatBlock(TestStats.Profile((null, 1))));
        }

        [Test]
        public void Constructor_DuplicateStat_Throws() {
            var defence = TestStats.Create("Defence");
            Assert.Throws<ArgumentException>(() => new StatBlock(TestStats.Profile((defence, 1), (defence, 2))));
        }

        [Test]
        public void Calculate_DerivationCycle_Terminates() {
            var a = TestStats.Create("A");
            var b = TestStats.Create("B");
            TestStats.Derive(a, b, AmountModifier.Raw(1));
            TestStats.Derive(b, a, AmountModifier.Raw(1));
            var block = new StatBlock(TestStats.Profile((a, 1)));

            Assert.DoesNotThrow(() => block.Calculate());
        }
    }
}
