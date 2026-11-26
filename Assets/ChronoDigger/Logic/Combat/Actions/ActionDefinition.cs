using System.Collections.Generic;

namespace ChronoDigger.Combat {
    // [CreateAssetMenu(menuName = "Combat/Action Definition")]
    public class ActionDefinition /* : ScriptableObject */ {
        public string DisplayName;
        public int Windup;   // ticks before firing; 0 fires on the next tick
        public int Cooldown; // ticks after firing before usable again

        // [SerializeReference] on the next three in Unity, so subclasses can be picked in the inspector.
        public TargetingRule Targeting;
        public List<ActionCost> Costs = new List<ActionCost>();
        public List<ActionEffect> Effects = new List<ActionEffect>();

        public override string ToString() => DisplayName;
    }
}
