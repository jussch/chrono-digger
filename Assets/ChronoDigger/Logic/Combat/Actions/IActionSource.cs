using System.Collections.Generic;
using ChronoDigger.Utils;

namespace ChronoDigger.Combat {
    public interface IActionSource {
        IEnumerable<ActionDefinition> GrantedActions { get; }
        SlotState GrantedActionState { get; }
    }
}
