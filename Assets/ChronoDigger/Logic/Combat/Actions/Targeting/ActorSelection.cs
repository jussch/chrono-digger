using System;
using System.Collections.Generic;

namespace ChronoDigger.Combat {
    /// <summary>Follows the actor wherever they move. Resolves empty once they leave the combat.</summary>
    public class ActorSelection : TargetSelection {
        public Actor Actor { get; }

        public ActorSelection(Actor actor) {
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
        }

        public override IReadOnlyList<Actor> Resolve(CombatState state) {
            return state.Contains(Actor) ? new[] { Actor } : Array.Empty<Actor>();
        }
    }
}
