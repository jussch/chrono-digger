using System.Collections.Generic;
using ChronoDigger.Combat.Stats;
using Marple.Core;

namespace ChronoDigger.Tests.Stats
{
    /// <summary>
    /// The one place tests construct their own definitions and profiles.
    /// In Unity this becomes ScriptableObject.CreateInstance.
    /// </summary>
    static class TestStats
    {
        public static StatDefinition Create(string name, double defaultValue = 0)
            => new StatDefinition { displayName = name, defaultValue = defaultValue };

        public static StatDerivation Derive(StatDefinition source, StatDefinition target, AmountModifier perPoint)
        {
            var derivation = new StatDerivation { target = target, perPoint = perPoint };
            source.derivations.Add(derivation);
            return derivation;
        }

        public static StatProfile Profile(params (StatDefinition stat, double value)[] overrides)
        {
            var profile = new StatProfile { displayName = "Test Profile", overrides = new List<StatEntry>() };
            foreach (var (stat, value) in overrides)
                profile.overrides.Add(new StatEntry { stat = stat, baseValue = value });
            return profile;
        }
    }
}
