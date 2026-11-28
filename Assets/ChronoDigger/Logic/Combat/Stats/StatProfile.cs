using System;
using System.Collections.Generic;

namespace ChronoDigger.Combat {
    /// <summary>
    /// Authored template for an actor type ("what a goblin shaman is"). Stores only
    /// overrides; any stat not listed uses its <see cref="StatDefinition.DefaultValue"/>.
    /// A list rather than a dictionary because Unity can't serialize dictionaries.
    /// </summary>
    [Serializable]
    public class StatProfile {
        // Odin: [TableList] for a compact grid, and
        // [ValidateInput(...)] flagging null and duplicate stats (StatTable throws on both).
        public List<StatEntry> Entries = new List<StatEntry>();
    }
}
