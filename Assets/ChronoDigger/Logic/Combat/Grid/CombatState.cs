using System;
using System.Collections.Generic;

namespace ChronoDigger.Combat {
    public class CombatState {
        private readonly List<Actor> _actors = new List<Actor>();

        public Actor Player { get; }
        /// <summary>Every actor in the combat, including the player.</summary>
        public IReadOnlyList<Actor> Actors => _actors;

        public CombatState(Actor player, IEnumerable<Actor> others) {
            if (player == null) throw new ArgumentNullException(nameof(player));
            Player = player;
            _actors.Add(player);
            if (others != null) _actors.AddRange(others);
        }

        public bool Contains(Actor actor) => _actors.Contains(actor);

        public void Remove(Actor actor) {
            _actors.Remove(actor);
        }
    }
}
