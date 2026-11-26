using System;

namespace ChronoDigger.Combat {
    /// <summary>Uses up the granting source itself, e.g. drinking a potion from a stack.</summary>
    [Serializable]
    public class ConsumeSourceCost : ActionCost {
        public int Amount = 1;

        public override bool CanPay(ActionContext context) {
            return context.Slot.Source is IConsumableSource consumable && consumable.CanConsume(Amount);
        }

        public override void Pay(ActionContext context) {
            ((IConsumableSource)context.Slot.Source).Consume(Amount);
        }
    }
}
