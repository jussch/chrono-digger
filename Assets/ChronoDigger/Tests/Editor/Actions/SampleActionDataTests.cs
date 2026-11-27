using ChronoDigger.Combat;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class SampleActionDataTests {
        [Test]
        public void SwordActions_GrantsSwingAndParry() {
            var set = new ActionSet();

            set.Rebuild(new[] { new FakeActionSource(SampleActionData.SwordActions) });

            Assert.AreEqual(2, set.Slots.Count);
            Assert.AreSame(SampleActionData.SwingSword, set.Slots[0].Definition);
            Assert.AreSame(SampleActionData.Parry, set.Slots[1].Definition);
        }

        [Test]
        public void SampleActions_MatchDesignValues() {
            Assert.AreEqual(1, SampleActionData.SwingSword.Windup.GetValue(new ParameterTable()).ToInt());
            Assert.AreEqual(0, SampleActionData.SwingSword.Cooldown.GetValue(new ParameterTable()).ToInt());
            Assert.AreEqual(1, ((SingleActorTargeting)SampleActionData.SwingSword.Targeting).Range);
            Assert.AreEqual(3, SampleActionData.CastFireball.Windup.GetValue(new ParameterTable()).ToInt());
            Assert.AreEqual(2, SampleActionData.CastFireball.Cooldown.GetValue(new ParameterTable()).ToInt());
            Assert.AreEqual(3, ((SingleActorTargeting)SampleActionData.CastFireball.Targeting).Range);
            Assert.IsInstanceOf<SelfTargeting>(SampleActionData.DrinkHealingPotion.Targeting);
            Assert.IsInstanceOf<ConsumeSourceCost>(SampleActionData.DrinkHealingPotion.Costs[0]);
        }
    }
}
