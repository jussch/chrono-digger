using System;

namespace ChronoDigger.Combat {
    /// <summary>One other actor in the combat, within <see cref="Range"/> rows.</summary>
    [Serializable]
    public class SingleActorTargeting : TargetingRule {
        public int Range;

        public override bool IsValid(Actor user, TargetSelection selection, CombatState state) {
            return selection is ActorSelection actorSelection
                && actorSelection.Actor != user
                && state.Contains(actorSelection.Actor)
                && GridPos.RowDistance(user.Position, actorSelection.Actor.Position) <= Range;
        }
    }
}
