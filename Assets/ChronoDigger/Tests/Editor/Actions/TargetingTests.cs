using ChronoDigger.Combat;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class TargetingTests {
        [Test]
        public void SingleActorTargeting_EnemyInRowZeroRangeOne_ReachesPlayer() {
            var player = ActionTestHelpers.Player();
            var enemy = ActionTestHelpers.ActorAt(row: 0);
            var state = new CombatState(player, new[] { enemy });

            Assert.IsTrue(new SingleActorTargeting { Range = 1 }.IsValid(enemy, new ActorSelection(player), state));
        }

        [Test]
        public void SingleActorTargeting_EnemyInRowOneRangeOne_CannotReachPlayer() {
            var player = ActionTestHelpers.Player();
            var enemy = ActionTestHelpers.ActorAt(row: 1);
            var state = new CombatState(player, new[] { enemy });

            Assert.IsFalse(new SingleActorTargeting { Range = 1 }.IsValid(enemy, new ActorSelection(player), state));
        }

        [Test]
        public void SingleActorTargeting_DifferentColumnSameRow_UsesRowsOnly() {
            var player = ActionTestHelpers.Player();
            var enemy = ActionTestHelpers.ActorAt(row: 0, column: 7);
            var state = new CombatState(player, new[] { enemy });

            Assert.IsTrue(new SingleActorTargeting { Range = 1 }.IsValid(enemy, new ActorSelection(player), state));
        }

        [Test]
        public void SingleActorTargeting_Self_Invalid() {
            var player = ActionTestHelpers.Player();
            var state = new CombatState(player, new Actor[0]);

            Assert.IsFalse(new SingleActorTargeting { Range = 5 }.IsValid(player, new ActorSelection(player), state));
        }

        [Test]
        public void SingleActorTargeting_TargetNotInState_Invalid() {
            var player = ActionTestHelpers.Player();
            var stranger = ActionTestHelpers.ActorAt(row: 0);
            var state = new CombatState(player, new Actor[0]);

            Assert.IsFalse(new SingleActorTargeting { Range = 5 }.IsValid(player, new ActorSelection(stranger), state));
        }

        [Test]
        public void SelfTargeting_User_Valid() {
            var player = ActionTestHelpers.Player();
            var state = new CombatState(player, new Actor[0]);

            Assert.IsTrue(new SelfTargeting().IsValid(player, new ActorSelection(player), state));
        }

        [Test]
        public void SelfTargeting_OtherActor_Invalid() {
            var player = ActionTestHelpers.Player();
            var enemy = ActionTestHelpers.ActorAt(row: 0);
            var state = new CombatState(player, new[] { enemy });

            Assert.IsFalse(new SelfTargeting().IsValid(player, new ActorSelection(enemy), state));
        }

        [Test]
        public void ActorSelection_TargetRemoved_ResolvesEmpty() {
            var player = ActionTestHelpers.Player();
            var enemy = ActionTestHelpers.ActorAt(row: 0);
            var state = new CombatState(player, new[] { enemy });
            var selection = new ActorSelection(enemy);

            state.Remove(enemy);

            Assert.AreEqual(0, selection.Resolve(state).Count);
        }

        [Test]
        public void ActorSelection_TargetPresent_ResolvesToTarget() {
            var player = ActionTestHelpers.Player();
            var enemy = ActionTestHelpers.ActorAt(row: 0);
            var state = new CombatState(player, new[] { enemy });

            CollectionAssert.AreEqual(new[] { enemy }, new ActorSelection(enemy).Resolve(state));
        }
    }
}
