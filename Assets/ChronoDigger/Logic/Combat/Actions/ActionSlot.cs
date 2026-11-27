using System;

namespace ChronoDigger.Combat {
    /// <summary>Runtime state of one action granted to an actor by one source.</summary>
    public class ActionSlot {
        public ActionDefinition Definition { get; }
        public IActionSource Source { get; }
        public int CooldownRemaining { get; private set; }
        public bool IsReady => CooldownRemaining == 0;

        public ActionSlot(ActionDefinition definition, IActionSource source) {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (source == null) throw new ArgumentNullException(nameof(source));
            Definition = definition;
            Source = source;
        }

        public void StartCooldown(ActionContext context) => StartCooldown(context.Parameters);
        public void StartCooldown(ParameterTable table) {
            CooldownRemaining = Definition.Cooldown.GetValue(table).ToInt();
        }

        /// <summary>Clamps at 0.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is negative.</exception>
        public void AdvanceCooldown(int amount) {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Must not be negative.");
            CooldownRemaining = Math.Max(CooldownRemaining - amount, 0);
        }
    }
}
