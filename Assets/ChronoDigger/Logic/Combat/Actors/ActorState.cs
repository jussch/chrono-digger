namespace ChronoDigger.Combat.Actors {
    public class ActorState {
        public Amount health;
        public Amount mana;
        public Amount block;
        public Amount stamina;

        public Initiate(StatBlock statBlock) {
            health = statBlock.Get(Stat.MaxHealth);
            mana = statBlock.Get(Stat.MaxMana);
            // ...
        }
    }
}