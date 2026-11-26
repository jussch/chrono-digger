using System;

namespace ChronoDigger.Combat {
    [Serializable]
    public class ActorStateCost : ActionCost {
        public ActorStateValues StateValueType;
        public int Cost;

        public override bool CanPay(ActionContext context) {
            return context.User.GetStateValue(StateValueType) >= Cost;
        }

        public override void Pay(ActionContext context) {
            context.User.ChangeStateValue(StateValueType, -Cost);
        }
    }
}