using System;

namespace ChronoDigger.Combat {
    /// <summary>Authored: what an action is allowed to target.</summary>
    [Serializable]
    public abstract class TargetingRule {
        public abstract bool IsValid(Actor user, TargetSelection selection, CombatState state);
    }
}
