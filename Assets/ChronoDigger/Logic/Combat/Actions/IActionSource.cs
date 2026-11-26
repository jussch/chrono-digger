using System.Collections.Generic;

namespace ChronoDigger.Combat {
    public interface IActionSource {
        IEnumerable<ActionDefinition> GrantedActions { get; }
    }
}
