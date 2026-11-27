using ChronoDigger.Combat;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class ActorStatusEffectTests {
        private StatDefinition _armor;
        private StatDefinition _speed;

        [SetUp]
        public void SetUp() {
            _armor = TestStats.Create("Armor", 10);
            _speed = TestStats.Create("Speed", 4);
        }

        private static Actor CreateActor(StatProfile profile) {
            var actor = new Actor();
            actor.LoadStatTable(profile);
            actor.Initiate();
            return actor;
        }

        private StatusEffect ArmorBuff(int amount, int baseTime = 10) {
            var definition = TestStats.StatusEffect((_armor, AmountModifier.Raw(amount)));
            definition.BaseTime = baseTime;
            return new StatusEffect(definition);
        }

        [Test]
        public void AddStatusEffect_RawModifier_ChangesActorStat() {
            var actor = CreateActor(TestStats.Profile());

            actor.AddStatusEffect(ArmorBuff(5));

            Assert.AreEqual((Amount)15, actor.Stats.GetValue(_armor));
        }

        [Test]
        public void AddStatusEffect_OverriddenStat_ModifiesProfileValue() {
            var actor = CreateActor(TestStats.Profile((_armor, 30)));

            actor.AddStatusEffect(ArmorBuff(5));

            Assert.AreEqual((Amount)35, actor.Stats.GetValue(_armor));
        }

        [Test]
        public void AddStatusEffect_TracksEffectInState() {
            var actor = CreateActor(TestStats.Profile());
            var effect = ArmorBuff(5);

            actor.AddStatusEffect(effect);

            CollectionAssert.Contains(actor.StatusEffects, effect);
        }

        [Test]
        public void AddStatusEffect_TwoDifferentEffectsOnSameStat_BothApply() {
            var actor = CreateActor(TestStats.Profile());

            actor.AddStatusEffect(ArmorBuff(5));
            actor.AddStatusEffect(ArmorBuff(3));

            Assert.AreEqual((Amount)18, actor.Stats.GetValue(_armor));
        }

        [Test]
        public void AddStatusEffect_OtherStats_Unchanged() {
            var actor = CreateActor(TestStats.Profile());

            actor.AddStatusEffect(ArmorBuff(5));

            Assert.AreEqual((Amount)4, actor.Stats.GetValue(_speed));
            Assert.AreEqual(Stat.MaxHealth.DefaultValue, actor.Stats.GetValue(Stat.MaxHealth).ToDouble());
        }

        [Test]
        public void AddStatusEffect_SharedDefinition_OnlyModifiesTargetActor() {
            var target = CreateActor(TestStats.Profile());
            var bystander = CreateActor(TestStats.Profile());
            var definition = TestStats.StatusEffect((_armor, AmountModifier.Raw(5)));

            target.AddStatusEffect(new StatusEffect(definition));

            Assert.AreEqual((Amount)15, target.Stats.GetValue(_armor));
            Assert.AreEqual((Amount)10, bystander.Stats.GetValue(_armor));
        }

        [Test]
        public void RemoveStatusEffect_AfterAdd_RestoresStat() {
            var actor = CreateActor(TestStats.Profile());
            var effect = ArmorBuff(5);
            actor.AddStatusEffect(effect);

            actor.RemoveStatusEffect(effect);

            Assert.AreEqual((Amount)10, actor.Stats.GetValue(_armor));
            CollectionAssert.DoesNotContain(actor.StatusEffects, effect);
        }

        [Test]
        public void RemoveStatusEffect_OneOfTwo_KeepsTheOther() {
            var actor = CreateActor(TestStats.Profile());
            var first = ArmorBuff(5);
            actor.AddStatusEffect(first);
            actor.AddStatusEffect(ArmorBuff(3));

            actor.RemoveStatusEffect(first);

            Assert.AreEqual((Amount)13, actor.Stats.GetValue(_armor));
        }

        [Test]
        public void Increment_BeforeExpiry_KeepsModifier() {
            var actor = CreateActor(TestStats.Profile());
            var effect = ArmorBuff(5, baseTime: 10);
            actor.AddStatusEffect(effect);

            actor.Increment(9);

            Assert.AreEqual((Amount)15, actor.Stats.GetValue(_armor));
            CollectionAssert.Contains(actor.StatusEffects, effect);
        }

        [Test]
        public void Increment_PastExpiry_RemovesEffectAndRestoresStat() {
            var actor = CreateActor(TestStats.Profile());
            var effect = ArmorBuff(5, baseTime: 10);
            actor.AddStatusEffect(effect);

            actor.Increment(10);

            Assert.AreEqual((Amount)10, actor.Stats.GetValue(_armor));
            CollectionAssert.DoesNotContain(actor.StatusEffects, effect);
        }

        [Test]
        public void Increment_EffectsWithDifferentDurations_ExpireSeparately() {
            var actor = CreateActor(TestStats.Profile());
            actor.AddStatusEffect(ArmorBuff(5, baseTime: 5));
            actor.AddStatusEffect(ArmorBuff(3, baseTime: 10));

            actor.Increment(5);

            Assert.AreEqual((Amount)13, actor.Stats.GetValue(_armor));
            Assert.AreEqual(1, actor.StatusEffects.Count);
        }

        private static StatusEffectDefinition StackingArmorBuff(StatDefinition stat, int perStack, int maxStacks = 5) {
            var definition = TestStats.StatusEffect((stat, AmountModifier.Raw(perStack)));
            definition.StacksMultiplyModifiers = true;
            definition.MaxStacks = maxStacks;
            return definition;
        }

        [Test]
        public void AddStatusEffect_SameDefinitionTwice_KeepsOneEffect() {
            var actor = CreateActor(TestStats.Profile());
            var definition = TestStats.StatusEffect((_armor, AmountModifier.Raw(5)));

            actor.AddStatusEffect(new StatusEffect(definition));
            actor.AddStatusEffect(new StatusEffect(definition));

            Assert.AreEqual(1, actor.StatusEffects.Count);
            Assert.AreEqual((Amount)15, actor.Stats.GetValue(_armor));
        }

        [Test]
        public void AddStatusEffect_SameStackingDefinitionTwice_AppliesCombinedStacks() {
            var actor = CreateActor(TestStats.Profile());
            var definition = StackingArmorBuff(_armor, 5);

            actor.AddStatusEffect(new StatusEffect(definition, 2));
            actor.AddStatusEffect(new StatusEffect(definition, 1));

            Assert.AreEqual(3, actor.StatusEffects[0].Stacks);
            Assert.AreEqual((Amount)25, actor.Stats.GetValue(_armor));
        }

        [Test]
        public void AddStatusEffect_SameStackingDefinitionPastMax_ClampsStacks() {
            var actor = CreateActor(TestStats.Profile());
            var definition = StackingArmorBuff(_armor, 5, maxStacks: 3);

            actor.AddStatusEffect(new StatusEffect(definition, 2));
            actor.AddStatusEffect(new StatusEffect(definition, 2));

            Assert.AreEqual((Amount)25, actor.Stats.GetValue(_armor));
        }

        [Test]
        public void AddStatusEffect_SameDefinitionTwice_RefreshesDuration() {
            var actor = CreateActor(TestStats.Profile());
            var definition = TestStats.StatusEffect((_armor, AmountModifier.Raw(5)));
            definition.BaseTime = 10;
            actor.AddStatusEffect(new StatusEffect(definition));
            actor.Increment(8);

            actor.AddStatusEffect(new StatusEffect(definition));
            actor.Increment(8);

            Assert.AreEqual((Amount)15, actor.Stats.GetValue(_armor));
        }

        [Test]
        public void RemoveStatusEffect_AfterMergingSameDefinition_RestoresStat() {
            var actor = CreateActor(TestStats.Profile());
            var definition = StackingArmorBuff(_armor, 5);
            actor.AddStatusEffect(new StatusEffect(definition, 2));
            actor.AddStatusEffect(new StatusEffect(definition, 1));

            actor.RemoveStatusEffect(actor.StatusEffects[0]);

            Assert.AreEqual((Amount)10, actor.Stats.GetValue(_armor));
            Assert.AreEqual(0, actor.StatusEffects.Count);
        }

        [Test]
        public void AddStatusEffect_NewStacksOnSameDefinition_SharedAcrossActorsDoesNotLeak() {
            var first = CreateActor(TestStats.Profile());
            var second = CreateActor(TestStats.Profile());
            var definition = StackingArmorBuff(_armor, 5);

            first.AddStatusEffect(new StatusEffect(definition, 3));
            second.AddStatusEffect(new StatusEffect(definition, 1));

            Assert.AreEqual((Amount)25, first.Stats.GetValue(_armor));
            Assert.AreEqual((Amount)15, second.Stats.GetValue(_armor));
        }
    }
}
