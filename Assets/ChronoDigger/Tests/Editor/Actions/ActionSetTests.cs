using ChronoDigger.Combat;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class ActionSetTests {
        [Test]
        public void Rebuild_SourceGrantingTwoActions_CreatesTwoSlots() {
            var first = TestStats.Action(new SelfTargeting());
            var second = TestStats.Action(new SelfTargeting());
            var source = new FakeActionSource(first, second);
            var set = new ActionSet();

            set.Rebuild(new[] { source });

            Assert.AreEqual(2, set.Slots.Count);
            Assert.AreSame(first, set.Slots[0].Definition);
            Assert.AreSame(second, set.Slots[1].Definition);
            Assert.AreSame(source, set.Slots[0].Source);
        }

        [Test]
        public void Rebuild_PairStillPresent_KeepsSlotAndCooldown() {
            var action = TestStats.Action(new SelfTargeting(), cooldown: 3);
            var source = new FakeActionSource(action);
            var set = new ActionSet();
            set.Rebuild(new[] { source });
            var slot = set.Slots[0];
            slot.StartCooldown();

            set.Rebuild(new IActionSource[] { new FakeActionSource(TestStats.Action(new SelfTargeting())), source });

            Assert.AreEqual(2, set.Slots.Count);
            Assert.AreSame(slot, set.Slots[1]);
            Assert.AreEqual(3, set.Slots[1].CooldownRemaining);
        }

        [Test]
        public void Rebuild_SameDefinitionFromDifferentSource_CreatesFreshSlot() {
            var action = TestStats.Action(new SelfTargeting(), cooldown: 3);
            var set = new ActionSet();
            set.Rebuild(new[] { new FakeActionSource(action) });
            set.Slots[0].StartCooldown();

            set.Rebuild(new[] { new FakeActionSource(action) });

            Assert.IsTrue(set.Slots[0].IsReady);
        }

        [Test]
        public void Rebuild_SourceRemoved_DropsItsSlots() {
            var set = new ActionSet();
            set.Rebuild(new[] { new FakeActionSource(TestStats.Action(new SelfTargeting())) });

            set.Rebuild(new IActionSource[0]);

            Assert.AreEqual(0, set.Slots.Count);
        }
    }
}
