using ChronoDigger.Utils;
using System.Collections.Generic;

namespace ChronoDigger.Combat {
    public class EnemyDefinition : DescribableObject {
        public StatProfile Stats;
        public List<ActionDefinition> Actions;
    }
}