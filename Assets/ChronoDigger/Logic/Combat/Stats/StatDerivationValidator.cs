using System.Collections.Generic;

namespace ChronoDigger.Combat.Stats {
    /// <summary>Checks the derivation graph for cycles (A feeds B feeds A), which would recurse forever.</summary>
    public static class StatDerivationValidator {
        /// <summary>
        /// Walks every derivation reachable from <paramref name="start"/>. Returns the first cycle
        /// found as a path that begins and ends on the same stat (e.g. [A, B, A]), or null.
        /// The cycle may not include <paramref name="start"/> itself.
        /// </summary>
        public static List<StatDefinition> FindCycle(StatDefinition start) {
            return Visit(start, new List<StatDefinition>(), new HashSet<StatDefinition>());
        }

        static List<StatDefinition> Visit(StatDefinition stat, List<StatDefinition> path, HashSet<StatDefinition> cleared) {
            if (stat == null || cleared.Contains(stat)) return null;

            int index = path.IndexOf(stat);
            if (index >= 0) {
                var cycle = path.GetRange(index, path.Count - index);
                cycle.Add(stat);
                return cycle;
            }

            path.Add(stat);
            if (stat.derivations != null) {
                foreach (var derivation in stat.derivations) {
                    var cycle = Visit(derivation?.target, path, cleared);
                    if (cycle != null) return cycle;
                }
            }
            path.RemoveAt(path.Count - 1);
            cleared.Add(stat);
            return null;
        }
    }
}
