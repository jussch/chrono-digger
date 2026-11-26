using System;

namespace ChronoDigger.Combat {
    [Serializable]
    public abstract class ActionEffect {
        public abstract void Apply(ActionContext context, Actor target);
    }
}
