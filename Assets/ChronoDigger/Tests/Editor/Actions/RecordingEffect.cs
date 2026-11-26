using System.Collections.Generic;
using ChronoDigger.Combat;

namespace ChronoDigger.Tests {
    internal class RecordingEffect : ActionEffect {
        public readonly List<(Actor user, Actor target)> Applied = new List<(Actor user, Actor target)>();

        public override void Apply(ActionContext context, Actor target) {
            Applied.Add((context.User, target));
        }
    }
}
