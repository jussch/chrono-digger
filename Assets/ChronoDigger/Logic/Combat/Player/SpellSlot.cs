using System;
using System.Collections.Generic;
using ChronoDigger.Utils;

namespace ChronoDigger.Combat {
    [Serializable]
    public class SpellSlot : IActionSource {
        public SpellDefinition Definition;
        public SlotState SlotState;

        // IActionSource
        public IEnumerable<ActionDefinition> GrantedActions => Definition.ProvidedActions.Definitions;
        public SlotState GrantedActionState => SlotState;
    }
}