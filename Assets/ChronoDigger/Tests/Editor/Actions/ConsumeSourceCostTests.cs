using ChronoDigger.Combat;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class ConsumeSourceCostTests {
        private static ActionContext ContextFor(IActionSource source) {
            var user = new Actor();
            var slot = new ActionSlot(TestStats.Action(new SelfTargeting()), source);
            return new ActionContext(user, slot, new CombatState(user, new Actor[0]));
        }

        [Test]
        public void Pay_StackHasItems_ConsumesAmount() {
            var stack = new FakeConsumableSource(3);
            var cost = new ConsumeSourceCost { Amount = 2 };

            Assert.IsTrue(cost.CanPay(ContextFor(stack)));
            cost.Pay(ContextFor(stack));

            Assert.AreEqual(1, stack.Count);
        }

        [Test]
        public void CanPay_EmptyStack_False() {
            Assert.IsFalse(new ConsumeSourceCost().CanPay(ContextFor(new FakeConsumableSource(0))));
        }

        [Test]
        public void CanPay_NotConsumableSource_False() {
            Assert.IsFalse(new ConsumeSourceCost().CanPay(ContextFor(new FakeActionSource())));
        }
    }
}
