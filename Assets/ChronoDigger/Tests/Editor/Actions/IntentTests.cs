using System;
using ChronoDigger.Combat;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class IntentTests {
        private static Intent CreateIntent(int windup) {
            var actor = new Actor();
            var slot = new ActionSlot(TestStats.Action(new SelfTargeting()), new FakeActionSource());
            return new Intent(actor, slot, new ActorSelection(actor), windup);
        }

        [Test]
        public void Advance_PartWay_ReducesTimeRemaining() {
            var intent = CreateIntent(3);

            intent.Advance(1);

            Assert.AreEqual(2, intent.TimeRemaining);
            Assert.IsFalse(intent.IsReady);
        }

        [Test]
        public void Advance_PastZero_ClampsAtZero() {
            var intent = CreateIntent(2);

            intent.Advance(5);

            Assert.AreEqual(0, intent.TimeRemaining);
            Assert.IsTrue(intent.IsReady);
        }

        [Test]
        public void Advance_Negative_Throws() {
            var intent = CreateIntent(2);

            Assert.Throws<ArgumentOutOfRangeException>(() => intent.Advance(-1));
            Assert.AreEqual(2, intent.TimeRemaining);
        }
    }
}
