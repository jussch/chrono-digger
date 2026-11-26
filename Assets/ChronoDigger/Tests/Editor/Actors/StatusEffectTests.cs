using ChronoDigger.Combat;
using ChronoDigger.Tests.Stats;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests.Actors {
    public class StatusEffectTests {
        [Test]
        public void ApplyToStatBlock_RawModifier_ChangesStat() {
            var armor = TestStats.Create("Armor", 10);
            var block = new StatBlock(TestStats.Profile());
            var effect = new StatusEffect(TestStats.StatusEffect((armor, AmountModifier.Raw(5))));

            effect.ApplyToStatBlock(block);

            Assert.AreEqual((Amount)15, block.Get(armor));
        }

        [Test]
        public void ApplyToStatBlock_SeveralModifiers_ChangesEachStat() {
            var armor = TestStats.Create("Armor", 10);
            var speed = TestStats.Create("Speed", 4);
            var block = new StatBlock(TestStats.Profile());
            var effect = new StatusEffect(TestStats.StatusEffect(
                (armor, AmountModifier.Raw(5)),
                (speed, AmountModifier.Raw(-1))));

            effect.ApplyToStatBlock(block);

            Assert.AreEqual((Amount)15, block.Get(armor));
            Assert.AreEqual((Amount)3, block.Get(speed));
        }

        [Test]
        public void RemoveFromStatBlock_AfterApply_RestoresStat() {
            var armor = TestStats.Create("Armor", 10);
            var block = new StatBlock(TestStats.Profile((armor, 20)));
            var effect = new StatusEffect(TestStats.StatusEffect((armor, AmountModifier.Raw(5))));

            effect.ApplyToStatBlock(block);
            effect.RemoveFromStatBlock(block);

            Assert.AreEqual((Amount)20, block.Get(armor));
        }

        [Test]
        public void ApplyToStatBlock_StacksMultiplyModifiers_ScalesByStacks() {
            var armor = TestStats.Create("Armor", 10);
            var block = new StatBlock(TestStats.Profile());
            var definition = TestStats.StatusEffect((armor, AmountModifier.Raw(5)));
            definition.stacksMultiplyModifiers = true;
            definition.maxStacks = 5;
            var effect = new StatusEffect(definition);
            effect.AddStacks(3);

            effect.ApplyToStatBlock(block);

            Assert.AreEqual((Amount)25, block.Get(armor));
        }

        [Test]
        public void ApplyToStatBlock_StacksDoNotMultiplyModifiers_IgnoresStacks() {
            var armor = TestStats.Create("Armor", 10);
            var block = new StatBlock(TestStats.Profile());
            var definition = TestStats.StatusEffect((armor, AmountModifier.Raw(5)));
            definition.maxStacks = 5;
            var effect = new StatusEffect(definition);
            effect.AddStacks(3);

            effect.ApplyToStatBlock(block);

            Assert.AreEqual((Amount)15, block.Get(armor));
        }

        [Test]
        public void ApplyToStatBlock_StackingModifier_LeavesDefinitionUnchanged() {
            var armor = TestStats.Create("Armor", 10);
            var block = new StatBlock(TestStats.Profile());
            var definition = TestStats.StatusEffect((armor, AmountModifier.Raw(5)));
            definition.stacksMultiplyModifiers = true;
            definition.maxStacks = 5;
            AmountModifier original = definition.statModifiers[0].modifier;
            var effect = new StatusEffect(definition);
            effect.AddStacks(3);

            effect.ApplyToStatBlock(block);

            Assert.AreEqual(original, definition.statModifiers[0].modifier);
        }

        [Test]
        public void AddStacks_AboveMax_ClampsToMax() {
            var definition = TestStats.StatusEffect();
            definition.maxStacks = 3;
            var effect = new StatusEffect(definition);

            effect.AddStacks(5);

            Assert.AreEqual(3, effect.stacks);
        }

        [Test]
        public void IsExpired_IncrementedPastBaseTime_ReturnsTrue() {
            var definition = TestStats.StatusEffect();
            definition.baseTime = 10;
            var effect = new StatusEffect(definition);

            effect.Increment(9);
            Assert.IsFalse(effect.IsExpired());

            effect.Increment(1);
            Assert.IsTrue(effect.IsExpired());
        }
    }
}
