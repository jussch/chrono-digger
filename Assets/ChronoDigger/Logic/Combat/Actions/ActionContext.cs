namespace ChronoDigger.Combat {
    public class ActionContext {
        public Actor User { get; }
        public ActionSlot Slot { get; }
        public CombatState State { get; }
        public ParameterTable Parameters { get; private set; }

        public ActionContext(Actor user, ActionSlot slot, CombatState state) {
            User = user;
            Slot = slot;
            State = state;
            GenerateParameters();
        }

        public void GenerateParameters() {
            Parameters = Slot.Definition.ParameterProfile.Tabulate(User.Stats);
        }
    }
}
