using System;
using System.Collections.Generic;
using ChronoDigger.Utils;
using Marple.Core;

namespace ChronoDigger.Combat {
    public class ParameterTable : AmountTable<ParameterDefinition> {
        public ParameterTable() {
            
        }

        /// <exception cref="ArgumentException">The profile lists a null parameter or the same parameter twice.</exception>
        public ParameterTable(ParameterProfile profile, StatTable statTable) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            foreach (var entry in profile.Parameters ?? new List<ParameterProfile.Entry>()) {
                AddEntry(entry.Parameter, entry.BaseValue, profile);
                ModifiableAmount amount = Get(entry.Parameter);
                foreach (var statMod in entry.StatModifiers ?? new List<StatModifier>()) {
                    amount.Add(statMod.Stat, statMod.GetFullModifier(statTable.GetValue(statMod.Stat)).With(statMod.Stat));
                }
            }
        }

        protected override double DefaultValue(ParameterDefinition parameter) => parameter.DefaultValue;
    }
}
