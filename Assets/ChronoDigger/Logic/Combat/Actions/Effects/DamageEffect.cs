using System;
using Marple.Core;

namespace ChronoDigger.Combat {
    [Serializable]
    public abstract class DamageEffect : ActionEffect {
        public ParameterValue Value;
        public ParameterValue Repeats;
        public DamageDescriptor DamageDescriptor;

        public override void Apply(ActionContext context, Actor target) {
            Amount damage = Value.GetValue(context);
            int repeats = ClampRepeats(Repeats.GetValue(context));
            for (int i = 0; i < repeats; i++) {
                target.TakeDamage(DamageDescriptor.DefineInfo(damage, context.User));
            }
        }

        private static int ClampRepeats(Amount repeats) => Math.Max(1, repeats.ToInt());
    }
}
