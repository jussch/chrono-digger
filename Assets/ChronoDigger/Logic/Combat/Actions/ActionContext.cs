namespace ChronoDigger.Combat {
    public class ActionContext {
        public Actor User { get; }
        public ActionSlot Slot { get; }
        public CombatState State { get; }

        public ActionContext(Actor user, ActionSlot slot, CombatState state) {
            User = user;
            Slot = slot;
            State = state;
        }
    }
}
