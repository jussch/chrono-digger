using System;

namespace ChronoDigger.Combat {
    /// <summary>A declared action waiting to fire.</summary>
    public class Intent {
        public Actor Owner { get; }
        public ActionSlot Slot { get; }
        public TargetSelection Targets { get; }
        public int TimeRemaining { get; private set; }
        
        public bool IsReady => TimeRemaining == 0;

        public Intent(Actor owner, ActionSlot slot, TargetSelection targets, int timeRemaining) {
            Owner = owner;
            Slot = slot;
            Targets = targets;
            TimeRemaining = Math.Max(timeRemaining, 0);
        }

        /// <summary>The only way <see cref="TimeRemaining"/> changes. Clamps at 0.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is negative.</exception>
        public void Advance(int amount) {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Must not be negative.");
            TimeRemaining = Math.Max(TimeRemaining - amount, 0);
        }
    }
}
