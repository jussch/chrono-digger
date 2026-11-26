using System;
using ChronoDigger.Combat;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class ActionRulesTests {
        private Actor _player;
        private Actor _enemy;
        private CombatState _state;

        [SetUp]
        public void SetUp() {
            _player = ActionTestHelpers.Player();
            _enemy = ActionTestHelpers.ActorAt(row: 0);
            _state = new CombatState(_player, new[] { _enemy });
        }

        private static ActionSlot SlotFor(ActionDefinition definition) {
            return new ActionSlot(definition, new FakeActionSource(definition));
        }

        private static ActionDefinition Melee(int windup = 0, int cooldown = 0, params ActionCost[] costs) {
            return TestStats.Action(new SingleActorTargeting { Range = 1 }, windup, cooldown, costs);
        }

        [Test]
        public void TryDeclare_Valid_SetsIntentWithWindup() {
            var slot = SlotFor(Melee(windup: 2));

            var result = ActionRules.TryDeclare(_enemy, slot, new ActorSelection(_player), _state, out var intent);

            Assert.AreEqual(DeclareResult.Success, result);
            Assert.AreSame(intent, _enemy.CurrentIntent);
            Assert.AreEqual(2, intent.TimeRemaining);
            Assert.AreSame(slot, intent.Slot);
        }

        [Test]
        public void TryDeclare_UserAlreadyHasIntent_FailsAlreadyHasIntent() {
            var slot = SlotFor(Melee());
            ActionRules.TryDeclare(_enemy, slot, new ActorSelection(_player), _state, out var first);

            var result = ActionRules.TryDeclare(_enemy, SlotFor(Melee()), new ActorSelection(_player), _state, out var second);

            Assert.AreEqual(DeclareResult.AlreadyHasIntent, result);
            Assert.IsNull(second);
            Assert.AreSame(first, _enemy.CurrentIntent);
        }

        [Test]
        public void TryDeclare_SlotOnCooldown_FailsOnCooldown() {
            var slot = SlotFor(Melee(cooldown: 2));
            slot.StartCooldown();

            var result = ActionRules.TryDeclare(_enemy, slot, new ActorSelection(_player), _state, out _);

            Assert.AreEqual(DeclareResult.OnCooldown, result);
            Assert.IsNull(_enemy.CurrentIntent);
        }

        [Test]
        public void TryDeclare_TargetOutOfRange_FailsInvalidTarget() {
            var farEnemy = ActionTestHelpers.ActorAt(row: 1);
            var farState = new CombatState(_player, new[] { farEnemy });

            var result = ActionRules.TryDeclare(farEnemy, SlotFor(Melee()), new ActorSelection(_player), farState, out _);

            Assert.AreEqual(DeclareResult.InvalidTarget, result);
            Assert.IsNull(farEnemy.CurrentIntent);
        }

        [Test]
        public void TryDeclare_CostCannotBePaid_FailsCannotPay() {
            var cost = new FakeCost { Payable = false };

            var result = ActionRules.TryDeclare(_enemy, SlotFor(Melee(costs: cost)), new ActorSelection(_player), _state, out _);

            Assert.AreEqual(DeclareResult.CannotPay, result);
            Assert.IsNull(_enemy.CurrentIntent);
        }

        [Test]
        public void TryDeclare_Success_PaysEveryCostOnce() {
            var first = new FakeCost();
            var second = new FakeCost();

            ActionRules.TryDeclare(_enemy, SlotFor(Melee(costs: new ActionCost[] { first, second })), new ActorSelection(_player), _state, out _);

            Assert.AreEqual(1, first.TimesPaid);
            Assert.AreEqual(1, second.TimesPaid);
        }

        [Test]
        public void TryDeclare_LaterCostUnpayable_PaysNothing() {
            var payable = new FakeCost();
            var unpayable = new FakeCost { Payable = false };

            ActionRules.TryDeclare(_enemy, SlotFor(Melee(costs: new ActionCost[] { payable, unpayable })), new ActorSelection(_player), _state, out _);

            Assert.AreEqual(0, payable.TimesPaid);
            Assert.AreEqual(0, unpayable.TimesPaid);
        }

        [Test]
        public void TryDeclare_InvalidTarget_PaysNothing() {
            var cost = new FakeCost();

            ActionRules.TryDeclare(_enemy, SlotFor(Melee(costs: cost)), new ActorSelection(_enemy), _state, out _);

            Assert.AreEqual(0, cost.TimesPaid);
        }

        [Test]
        public void TryDeclare_ConsumeSourceCost_UsesUpStackUntilEmpty() {
            var potion = TestStats.Action(new SelfTargeting(), costs: new ConsumeSourceCost());
            var stack = new FakeConsumableSource(1, potion);
            var slot = new ActionSlot(potion, stack);

            var first = ActionRules.TryDeclare(_player, slot, new ActorSelection(_player), _state, out var intent);
            ActionRules.Fire(intent, _state);
            var second = ActionRules.TryDeclare(_player, slot, new ActorSelection(_player), _state, out _);

            Assert.AreEqual(DeclareResult.Success, first);
            Assert.AreEqual(DeclareResult.CannotPay, second);
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void TryDeclare_Success_DoesNotStartCooldown() {
            var slot = SlotFor(Melee(cooldown: 2));

            ActionRules.TryDeclare(_enemy, slot, new ActorSelection(_player), _state, out _);

            Assert.IsTrue(slot.IsReady);
        }

        [Test]
        public void Fire_Ready_StartsCooldown() {
            var slot = SlotFor(Melee(cooldown: 2));
            ActionRules.TryDeclare(_enemy, slot, new ActorSelection(_player), _state, out var intent);

            ActionRules.Fire(intent, _state);

            Assert.AreEqual(2, slot.CooldownRemaining);
        }

        [Test]
        public void Fire_NotReady_Throws() {
            ActionRules.TryDeclare(_enemy, SlotFor(Melee(windup: 1)), new ActorSelection(_player), _state, out var intent);

            Assert.Throws<InvalidOperationException>(() => ActionRules.Fire(intent, _state));
        }

        [Test]
        public void Fire_AfterAdvancingWindup_Fires() {
            var effect = new RecordingEffect();
            var definition = Melee(windup: 2);
            definition.Effects.Add(effect);
            ActionRules.TryDeclare(_enemy, SlotFor(definition), new ActorSelection(_player), _state, out var intent);

            intent.Advance(2);
            ActionRules.Fire(intent, _state);

            Assert.AreEqual(1, effect.Applied.Count);
        }

        [Test]
        public void Fire_AppliesEveryEffectToTargetThenClearsIntent() {
            var first = new RecordingEffect();
            var second = new RecordingEffect();
            var definition = Melee();
            definition.Effects.Add(first);
            definition.Effects.Add(second);
            ActionRules.TryDeclare(_enemy, SlotFor(definition), new ActorSelection(_player), _state, out var intent);

            ActionRules.Fire(intent, _state);

            CollectionAssert.AreEqual(new[] { (_enemy, _player) }, first.Applied);
            CollectionAssert.AreEqual(new[] { (_enemy, _player) }, second.Applied);
            Assert.IsNull(_enemy.CurrentIntent);
        }

        [Test]
        public void Fire_TargetRemoved_FizzlesButStillCompletes() {
            var effect = new RecordingEffect();
            var definition = Melee(cooldown: 1);
            definition.Effects.Add(effect);
            var slot = SlotFor(definition);
            ActionRules.TryDeclare(_player, slot, new ActorSelection(_enemy), _state, out var intent);
            _state.Remove(_enemy);

            ActionRules.Fire(intent, _state);

            Assert.AreEqual(0, effect.Applied.Count);
            Assert.AreEqual(1, slot.CooldownRemaining);
            Assert.IsNull(_player.CurrentIntent);
        }
    }
}
