using System.Collections.Generic;
using ChronoDigger.Combat;
using Marple.Core;

namespace ChronoDigger.Tests {
    /// <summary>
    /// The one place tests construct their own definitions, profiles, status effect and action definitions.
    /// In Unity this becomes ScriptableObject.CreateInstance.
    /// </summary>
    internal static class TestStats {
        public static StatDefinition Create(string name, double defaultValue = 0)
            => new StatDefinition { DisplayName = name, DefaultValue = defaultValue };

        public static StatModifier Derive(StatDefinition source, StatDefinition stat, AmountModifier modifier) {
            var derivation = new StatModifier { Stat = stat, Modifier = modifier };
            source.Derivations.Add(derivation);
            return derivation;
        }

        public static StatProfile Profile(params (StatDefinition stat, double value)[] overrides) {
            var profile = new StatProfile { Entries = new List<StatEntry>() };
            foreach (var (stat, value) in overrides)
                profile.Entries.Add(new StatEntry { Stat = stat, BaseValue = value });
            return profile;
        }

        public static ParameterDefinition Parameter(string name, double defaultValue = 0)
            => new ParameterDefinition { DisplayName = name, DefaultValue = defaultValue };

        public static ParameterProfile ParameterProfile(params (ParameterDefinition parameter, double value)[] entries) {
            var profile = new ParameterProfile { Parameters = new List<ParameterProfile.Entry>() };
            foreach (var (parameter, value) in entries)
                profile.Parameters.Add(new ParameterProfile.Entry { Parameter = parameter, BaseValue = value });
            return profile;
        }

        public static StatusEffectDefinition StatusEffect(params (StatDefinition stat, AmountModifier modifier)[] modifiers) {
            var definition = new StatusEffectDefinition { StatModifiers = new List<StatModifier>() };
            foreach (var (stat, modifier) in modifiers)
                definition.StatModifiers.Add(new StatModifier { Stat = stat, Modifier = modifier });
            return definition;
        }

        public static ActionDefinition Action(TargetingRule targeting, int windup = 0, int cooldown = 0,
                params ActionCost[] costs) {
            return new ActionDefinition {
                DisplayName = "Test Action", Targeting = targeting, Windup = windup, Cooldown = cooldown,
                Costs = new List<ActionCost>(costs),
            };
        }
    }
}
