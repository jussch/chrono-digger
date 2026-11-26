using System;

namespace ChronoDigger.Combat {
    [Serializable]
    public abstract class ActionCost {
        public abstract bool CanPay(ActionContext context);
        public abstract void Pay(ActionContext context);
    }
}
