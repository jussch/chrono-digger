using System;
using System.Collections.Generic;
using ChronoDigger.Utils;

namespace ChronoDigger.Combat {
    public class ParameterTable : AmountTable<ParameterDefinition> {
        /// <exception cref="ArgumentException">The profile lists a null parameter or the same parameter twice.</exception>
        public ParameterTable(ParameterProfile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            foreach (var entry in profile.Parameters ?? new List<ParameterProfile.Entry>()) {
                AddEntry(entry.Parameter, entry.BaseValue, profile);
            }
        }

        protected override double DefaultValue(ParameterDefinition parameter) => parameter.DefaultValue;
    }
}
