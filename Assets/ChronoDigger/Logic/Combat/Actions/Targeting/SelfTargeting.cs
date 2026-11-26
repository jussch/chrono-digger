using System;

namespace ChronoDigger.Combat {
    [Serializable]
    public class SelfTargeting : TargetingRule {
        public override bool IsValid(Actor user, TargetSelection selection, CombatState state) {
            return selection is ActorSelection actorSelection && actorSelection.Actor == user;
        }
    }
}
