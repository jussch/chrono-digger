using ChronoDigger.Combat.Stats;

namespace ChronoDigger.Combat.Actors {
    public class Actor {
        public ActorState state;
        public StatBlock statBlock;

        public void LoadStatBlock(StatProfile profile) {
            statBlock = new StatBlock(profile);
        }

        public void Initiate() {
            state = new ActorState(statBlock);
        }
    }
}