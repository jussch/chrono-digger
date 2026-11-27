using System;
using ChronoDigger.Combat;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class ActionSlotTests {
        private static ActionSlot Slot(int cooldown) {
            return new ActionSlot(TestStats.Action(new SelfTargeting(), cooldown: cooldown), new FakeActionSource());
        }

        private static ParameterTable Parameters() => new ParameterTable();

        [Test]
        public void StartCooldown_SetsToDefinitionCooldown() {
            var slot = Slot(cooldown: 2);

            slot.StartCooldown(Parameters());

            Assert.AreEqual(2, slot.CooldownRemaining);
            Assert.IsFalse(slot.IsReady);
        }

        [Test]
        public void AdvanceCooldown_PastZero_ClampsAtZero() {
            var slot = Slot(cooldown: 2);
            slot.StartCooldown(Parameters());

            slot.AdvanceCooldown(5);

            Assert.AreEqual(0, slot.CooldownRemaining);
            Assert.IsTrue(slot.IsReady);
        }

        [Test]
        public void AdvanceCooldown_Negative_Throws() {
            Assert.Throws<ArgumentOutOfRangeException>(() => Slot(cooldown: 2).AdvanceCooldown(-1));
        }
    }
}
