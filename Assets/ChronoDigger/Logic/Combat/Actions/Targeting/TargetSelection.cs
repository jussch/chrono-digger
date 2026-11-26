using System.Collections.Generic;

namespace ChronoDigger.Combat {
    /// <summary>Runtime: what was chosen. Resolved to actors when the action fires.</summary>
    public abstract class TargetSelection {
        public abstract IReadOnlyList<Actor> Resolve(CombatState state);
    }
}
