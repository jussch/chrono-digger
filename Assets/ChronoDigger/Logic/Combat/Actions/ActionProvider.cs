using System;
using System.Collections.Generic;

namespace ChronoDigger.Combat {
    // Wrapper for a list of actions to provider
    // Later will add simplicty functions for Unity Inspector to easy this.
    [Serializable]
    public class ActionProvider {
        public List<ActionDefinition> Definitions;
    }
}