using ChronoDigger.Combat;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class StatusEffectTests {
        [Test]
        public void ApplyToStatTable_RawModifier_ChangesStat() {
            var armor = TestStats.Create("Armor", 10);
            var block = new StatTable(TestStats.Profile());
            var effect = new StatusEffect(TestStats.StatusEffect((armor, AmountModifier.Raw(5))));

            effect.ApplyToStatTable(block);

            Assert.AreEqual((Amount)15, block.GetValue(armor));
        }

        [Test]
        public void ApplyToStatTable_SeveralModifiers_ChangesEachStat() {
            var armor = TestStats.Create("Armor", 10);
            var speed = TestStats.Create("Speed", 4);
            var block = new StatTable(TestStats.Profile());
            var effect = new StatusEffect(TestStats.StatusEffect(
                (armor, AmountModifier.Raw(5)),
                (speed, AmountModifier.Raw(-1))));

            effect.ApplyToStatTable(block);

            Assert.AreEqual((Amount)15, block.GetValue(armor));
            Assert.AreEqual((Amount)3, block.GetValue(speed));
        }

        [Test]
        public void RemoveFromStatTable_AfterApply_RestoresStat() {
            var armor = TestStats.Create("Armor", 10);
            var block = new StatTable(TestStats.Profile((armor, 20)));
            var effect = new StatusEffect(TestStats.StatusEffect((armor, AmountModifier.Raw(5))));

            effect.ApplyToStatTable(block);
            effect.RemoveFromStatTable(block);

            Assert.AreEqual((Amount)20, block.GetValue(armor));
        }

        [Test]
        public void ApplyToStatTable_StacksMultiplyModifiers_ScalesByStacks() {
            var armor = TestStats.Create("Armor", 10);
            var block = new StatTable(TestStats.Profile());
            var definition = TestStats.StatusEffect((armor, AmountModifier.Raw(5)));
            definition.StacksMultiplyModifiers = true;
            definition.MaxStacks = 5;
            var effect = new StatusEffect(definition);
            effect.AddStacks(3);

            effect.ApplyToStatTable(block);

            Assert.AreEqual((Amount)25, block.GetValue(armor));
        }

        [Test]
        public void ApplyToStatTable_StacksDoNotMultiplyModifiers_IgnoresStacks() {
            var armor = TestStats.Create("Armor", 10);
            var block = new StatTable(TestStats.Profile());
            var definition = TestStats.StatusEffect((armor, AmountModifier.Raw(5)));
            definition.MaxStacks = 5;
            var effect = new StatusEffect(definition);
            effect.AddStacks(3);

            effect.ApplyToStatTable(block);

            Assert.AreEqual((Amount)15, block.GetValue(armor));
        }

        [Test]
        public void ApplyToStatTable_StackingModifier_LeavesDefinitionUnchanged() {
            var armor = TestStats.Create("Armor", 10);
            var block = new StatTable(TestStats.Profile());
            var definition = TestStats.StatusEffect((armor, AmountModifier.Raw(5)));
            definition.StacksMultiplyModifiers = true;
            definition.MaxStacks = 5;
            AmountModifier original = definition.StatModifiers[0].Modifier;
            var effect = new StatusEffect(definition);
            effect.AddStacks(3);

            effect.ApplyToStatTable(block);

            Assert.AreEqual(original, definition.StatModifiers[0].Modifier);
        }

        [Test]
        public void AddStacks_AboveMax_ClampsToMax() {
            var definition = TestStats.StatusEffect();
            definition.MaxStacks = 3;
            var effect = new StatusEffect(definition);

            effect.AddStacks(5);

            Assert.AreEqual(3, effect.Stacks);
        }

        [Test]
        public void IsExpired_IncrementedPastBaseTime_ReturnsTrue() {
            var definition = TestStats.StatusEffect();
            definition.BaseTime = 10;
            var effect = new StatusEffect(definition);

            effect.Increment(9);
            Assert.IsFalse(effect.IsExpired());

            effect.Increment(1);
            Assert.IsTrue(effect.IsExpired());
        }
    }
}
