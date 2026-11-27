using System;
using Marple.Core;

namespace ChronoDigger.Combat {
    [Serializable]
    public class ApplyStatusEffect : ActionEffect {
        public StatusEffectDefinition StatusEffectDefinition;
        public ParameterValue Stacks;

        public override void Apply(ActionContext context, Actor target) {
            StatusEffect status = new StatusEffect(StatusEffectDefinition, Stacks.GetValue(context).ToInt(), context.User);
            target.AddStatusEffect(status);
        }
    }
}
