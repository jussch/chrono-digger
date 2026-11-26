using ChronoDigger.Combat;
using ChronoDigger.Tests.Stats;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests.Actors {
    public class ActorStatusEffectTests {
        private StatDefinition armor;
        private StatDefinition speed;

        [SetUp]
        public void SetUp() {
            armor = TestStats.Create("Armor", 10);
            speed = TestStats.Create("Speed", 4);
        }

        private static Actor CreateActor(StatProfile profile) {
            var actor = new Actor();
            actor.LoadStatBlock(profile);
            actor.Initiate();
            return actor;
        }

        private StatusEffect ArmorBuff(int amount, int baseTime = 10) {
            var definition = TestStats.StatusEffect((armor, AmountModifier.Raw(amount)));
            definition.baseTime = baseTime;
            return new StatusEffect(definition);
        }

        [Test]
        public void AddStatusEffect_RawModifier_ChangesActorStat() {
            var actor = CreateActor(TestStats.Profile());

            actor.AddStatusEffect(ArmorBuff(5));

            Assert.AreEqual((Amount)15, actor.statBlock.Get(armor));
        }

        [Test]
        public void AddStatusEffect_OverriddenStat_ModifiesProfileValue() {
            var actor = CreateActor(TestStats.Profile((armor, 30)));

            actor.AddStatusEffect(ArmorBuff(5));

            Assert.AreEqual((Amount)35, actor.statBlock.Get(armor));
        }

        [Test]
        public void AddStatusEffect_TracksEffectInState() {
            var actor = CreateActor(TestStats.Profile());
            var effect = ArmorBuff(5);

            actor.AddStatusEffect(effect);

            CollectionAssert.Contains(actor.state.statusEffects, effect);
        }

        [Test]
        public void AddStatusEffect_TwoDifferentEffectsOnSameStat_BothApply() {
            var actor = CreateActor(TestStats.Profile());

            actor.AddStatusEffect(ArmorBuff(5));
            actor.AddStatusEffect(ArmorBuff(3));

            Assert.AreEqual((Amount)18, actor.statBlock.Get(armor));
        }

        [Test]
        public void AddStatusEffect_OtherStats_Unchanged() {
            var actor = CreateActor(TestStats.Profile());

            actor.AddStatusEffect(ArmorBuff(5));

            Assert.AreEqual((Amount)4, actor.statBlock.Get(speed));
            Assert.AreEqual(Stat.MaxHealth.defaultValue, actor.statBlock.Get(Stat.MaxHealth).ToDouble());
        }

        [Test]
        public void AddStatusEffect_SharedDefinition_OnlyModifiesTargetActor() {
            var target = CreateActor(TestStats.Profile());
            var bystander = CreateActor(TestStats.Profile());
            var definition = TestStats.StatusEffect((armor, AmountModifier.Raw(5)));

            target.AddStatusEffect(new StatusEffect(definition));

            Assert.AreEqual((Amount)15, target.statBlock.Get(armor));
            Assert.AreEqual((Amount)10, bystander.statBlock.Get(armor));
        }

        [Test]
        public void RemoveStatusEffect_AfterAdd_RestoresStat() {
            var actor = CreateActor(TestStats.Profile());
            var effect = ArmorBuff(5);
            actor.AddStatusEffect(effect);

            actor.RemoveStatusEffect(effect);

            Assert.AreEqual((Amount)10, actor.statBlock.Get(armor));
            CollectionAssert.DoesNotContain(actor.state.statusEffects, effect);
        }

        [Test]
        public void RemoveStatusEffect_OneOfTwo_KeepsTheOther() {
            var actor = CreateActor(TestStats.Profile());
            var first = ArmorBuff(5);
            actor.AddStatusEffect(first);
            actor.AddStatusEffect(ArmorBuff(3));

            actor.RemoveStatusEffect(first);

            Assert.AreEqual((Amount)13, actor.statBlock.Get(armor));
        }

        [Test]
        public void Increment_BeforeExpiry_KeepsModifier() {
            var actor = CreateActor(TestStats.Profile());
            var effect = ArmorBuff(5, baseTime: 10);
            actor.AddStatusEffect(effect);

            actor.Increment(9);

            Assert.AreEqual((Amount)15, actor.statBlock.Get(armor));
            CollectionAssert.Contains(actor.state.statusEffects, effect);
        }

        [Test]
        public void Increment_PastExpiry_RemovesEffectAndRestoresStat() {
            var actor = CreateActor(TestStats.Profile());
            var effect = ArmorBuff(5, baseTime: 10);
            actor.AddStatusEffect(effect);

            actor.Increment(10);

            Assert.AreEqual((Amount)10, actor.statBlock.Get(armor));
            CollectionAssert.DoesNotContain(actor.state.statusEffects, effect);
        }

        [Test]
        public void Increment_EffectsWithDifferentDurations_ExpireSeparately() {
            var actor = CreateActor(TestStats.Profile());
            actor.AddStatusEffect(ArmorBuff(5, baseTime: 5));
            actor.AddStatusEffect(ArmorBuff(3, baseTime: 10));

            actor.Increment(5);

            Assert.AreEqual((Amount)13, actor.statBlock.Get(armor));
            Assert.AreEqual(1, actor.state.statusEffects.Count);
        }

        private static StatusEffectDefinition StackingArmorBuff(StatDefinition stat, int perStack, int maxStacks = 5) {
            var definition = TestStats.StatusEffect((stat, AmountModifier.Raw(perStack)));
            definition.stacksMultiplyModifiers = true;
            definition.maxStacks = maxStacks;
            return definition;
        }

        [Test]
        public void AddStatusEffect_SameDefinitionTwice_KeepsOneEffect() {
            var actor = CreateActor(TestStats.Profile());
            var definition = TestStats.StatusEffect((armor, AmountModifier.Raw(5)));

            actor.AddStatusEffect(new StatusEffect(definition));
            actor.AddStatusEffect(new StatusEffect(definition));

            Assert.AreEqual(1, actor.state.statusEffects.Count);
            Assert.AreEqual((Amount)15, actor.statBlock.Get(armor));
        }

        [Test]
        public void AddStatusEffect_SameStackingDefinitionTwice_AppliesCombinedStacks() {
            var actor = CreateActor(TestStats.Profile());
            var definition = StackingArmorBuff(armor, 5);

            actor.AddStatusEffect(new StatusEffect(definition, 2));
            actor.AddStatusEffect(new StatusEffect(definition, 1));

            Assert.AreEqual(3, actor.state.statusEffects[0].stacks);
            Assert.AreEqual((Amount)25, actor.statBlock.Get(armor));
        }

        [Test]
        public void AddStatusEffect_SameStackingDefinitionPastMax_ClampsStacks() {
            var actor = CreateActor(TestStats.Profile());
            var definition = StackingArmorBuff(armor, 5, maxStacks: 3);

            actor.AddStatusEffect(new StatusEffect(definition, 2));
            actor.AddStatusEffect(new StatusEffect(definition, 2));

            Assert.AreEqual((Amount)25, actor.statBlock.Get(armor));
        }

        [Test]
        public void AddStatusEffect_SameDefinitionTwice_RefreshesDuration() {
            var actor = CreateActor(TestStats.Profile());
            var definition = TestStats.StatusEffect((armor, AmountModifier.Raw(5)));
            definition.baseTime = 10;
            actor.AddStatusEffect(new StatusEffect(definition));
            actor.Increment(8);

            actor.AddStatusEffect(new StatusEffect(definition));
            actor.Increment(8);

            Assert.AreEqual((Amount)15, actor.statBlock.Get(armor));
        }

        [Test]
        public void RemoveStatusEffect_AfterMergingSameDefinition_RestoresStat() {
            var actor = CreateActor(TestStats.Profile());
            var definition = StackingArmorBuff(armor, 5);
            actor.AddStatusEffect(new StatusEffect(definition, 2));
            actor.AddStatusEffect(new StatusEffect(definition, 1));

            actor.RemoveStatusEffect(actor.state.statusEffects[0]);

            Assert.AreEqual((Amount)10, actor.statBlock.Get(armor));
            Assert.AreEqual(0, actor.state.statusEffects.Count);
        }

        [Test]
        public void AddStatusEffect_NewStacksOnSameDefinition_SharedAcrossActorsDoesNotLeak() {
            var first = CreateActor(TestStats.Profile());
            var second = CreateActor(TestStats.Profile());
            var definition = StackingArmorBuff(armor, 5);

            first.AddStatusEffect(new StatusEffect(definition, 3));
            second.AddStatusEffect(new StatusEffect(definition, 1));

            Assert.AreEqual((Amount)25, first.statBlock.Get(armor));
            Assert.AreEqual((Amount)15, second.statBlock.Get(armor));
        }
    }
}
