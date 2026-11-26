using System.Collections.Generic;
using ChronoDigger.Combat;

namespace ChronoDigger.Tests {
    internal class FakeConsumableSource : IConsumableSource {
        private readonly List<ActionDefinition> _actions;

        public int Count { get; private set; }

        public FakeConsumableSource(int count, params ActionDefinition[] actions) {
            Count = count;
            _actions = new List<ActionDefinition>(actions);
        }

        public IEnumerable<ActionDefinition> GrantedActions => _actions;
        public bool CanConsume(int amount) => Count >= amount;
        public void Consume(int amount) => Count -= amount;
    }
}
