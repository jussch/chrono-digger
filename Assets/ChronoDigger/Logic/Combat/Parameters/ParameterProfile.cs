using System;
using System.Collections.Generic;

namespace ChronoDigger.Combat {
    [Serializable]
    public class ParameterProfile {
        public List<Entry> Parameters = new List<Entry>();

        [Serializable]
        public struct Entry {
            public ParameterDefinition Parameter;
            public double BaseValue;

            public double RangeMin;
            public double RangeMax;

            // Todo: These probably want to include a variant of StatModifier
            // That handles fractions effectively.
            public List<StatModifier> statModifiers;
        }
    }
}