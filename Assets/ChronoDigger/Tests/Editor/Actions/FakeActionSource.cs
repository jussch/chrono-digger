using System.Collections.Generic;
using ChronoDigger.Combat;
using ChronoDigger.Utils;

namespace ChronoDigger.Tests {
    internal class FakeActionSource : IActionSource {
        private readonly List<ActionDefinition> _actions;

        public FakeActionSource(params ActionDefinition[] actions) {
            _actions = new List<ActionDefinition>(actions);
        }

        public IEnumerable<ActionDefinition> GrantedActions => _actions;
        public SlotState GrantedActionState { get; set; }
    }
}
