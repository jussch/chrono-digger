using System;

namespace ChronoDigger.Combat {
    [Serializable]
    public class ActorStateCost : ActionCost {
        public ActorStateValues StateValueType;
        public ParameterValue Cost;

        public override bool CanPay(ActionContext context) {
            return context.User.GetStateValue(StateValueType) >= Cost.GetValue(context).ToInt();
        }

        public override void Pay(ActionContext context) {
            context.User.ChangeStateValue(StateValueType, -Cost.GetValue(context).ToInt());
        }
    }
}