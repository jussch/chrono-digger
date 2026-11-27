using System.Collections.Generic;

namespace ChronoDigger.Combat {
    /// <summary>
    /// Authored template for an actor type ("what a goblin shaman is"). Stores only
    /// overrides; any stat not listed uses its <see cref="StatDefinition.DefaultValue"/>.
    /// A list rather than a dictionary because Unity can't serialize dictionaries.
    /// </summary>
    // [CreateAssetMenu(menuName = "Combat/Stat Profile")]
    public class StatProfile /* : ScriptableObject */ {
        public string DisplayName;

        // Odin: [TableList] for a compact grid, and
        // [ValidateInput(...)] flagging null and duplicate stats (StatTable throws on both).
        public List<StatEntry> Overrides = new List<StatEntry>();

        public override string ToString() => DisplayName;
    }
}
