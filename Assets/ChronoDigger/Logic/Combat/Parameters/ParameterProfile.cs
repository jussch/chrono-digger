using System;
using System.Collections.Generic;

namespace ChronoDigger.Combat {
    [Serializable]
    public class ParameterProfile {
        public List<Entry> Parameters = new List<Entry>();

        public ParameterTable Tabulate(StatTable stats) => new ParameterTable(this, stats);

        [Serializable]
        public struct Entry {
            public ParameterDefinition Parameter;
            public double BaseValue;
            public List<StatModifier> StatModifiers;
        }
    }
}