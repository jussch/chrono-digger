using System;

namespace ChronoDigger.Combat {
    public static class ActionRules {
        /// <summary>
        /// Checks everything before changing anything: on failure no cost is paid and no intent is set.
        /// On success pays every cost and sets <paramref name="intent"/> as the user's current intent.
        /// </summary>
        public static DeclareResult TryDeclare(Actor user, ActionSlot slot, TargetSelection targets, CombatState state, out Intent intent) {
            intent = null;
            var definition = slot.Definition;

            if (user.CurrentIntent != null) return DeclareResult.AlreadyHasIntent;
            if (!slot.IsReady) return DeclareResult.OnCooldown;
            if (definition.Targeting == null || targets == null || !definition.Targeting.IsValid(user, targets, state)) {
                return DeclareResult.InvalidTarget;
            }

            var context = new ActionContext(user, slot, state);
            foreach (var cost in definition.Costs) {
                if (!cost.CanPay(context)) return DeclareResult.CannotPay;
            }
            foreach (var cost in definition.Costs) {
                cost.Pay(context);
            }

            intent = new Intent(user, slot, targets, definition.Windup);
            user.CurrentIntent = intent;
            return DeclareResult.Success;
        }

        /// <summary>
        /// Resolves targets now, applies each effect to each target (no targets means a fizzle),
        /// starts the slot's cooldown and clears the owner's intent.
        /// </summary>
        /// <exception cref="InvalidOperationException">The intent isn't ready.</exception>
        public static void Fire(Intent intent, CombatState state) {
            if (!intent.IsReady) throw new InvalidOperationException($"Intent has {intent.TimeRemaining} time remaining.");

            var context = new ActionContext(intent.Owner, intent.Slot, state);
            var targets = intent.Targets.Resolve(state);
            foreach (var effect in intent.Slot.Definition.Effects) {
                foreach (var target in targets) {
                    effect.Apply(context, target);
                }
            }

            intent.Slot.StartCooldown();
            if (intent.Owner.CurrentIntent == intent) intent.Owner.CurrentIntent = null;
        }
    }
}
