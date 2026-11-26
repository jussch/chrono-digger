using System.Collections.Generic;

namespace ChronoDigger.Combat {
    public class ActionSet {
        private readonly List<ActionSlot> _slots = new List<ActionSlot>();

        public IReadOnlyList<ActionSlot> Slots => _slots;

        /// <summary>
        /// Creates one slot per granted action. A slot whose (definition, source) pair is still
        /// granted is kept, cooldown included, so re-equipping can't reset a cooldown.
        /// </summary>
        public void Rebuild(IEnumerable<IActionSource> sources) {
            var previous = new List<ActionSlot>(_slots);
            _slots.Clear();

            foreach (var source in sources) {
                foreach (var definition in source.GrantedActions) {
                    var existing = previous.Find(s => s.Definition == definition && s.Source == source);
                    if (existing != null) {
                        previous.Remove(existing);
                        _slots.Add(existing);
                    } else {
                        _slots.Add(new ActionSlot(definition, source));
                    }
                }
            }
        }
    }
}
