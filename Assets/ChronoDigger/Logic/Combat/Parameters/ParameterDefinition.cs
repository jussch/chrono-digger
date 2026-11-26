using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Combat {
    // [CreateAssetMenu(menuName = "Combat/Parameter Definition")]
    public class ParameterDefinition /* : ScriptableObject */ : IDescribable {
        // [field: SerializeField] on these two in Unity: they're auto-properties because IDescribable needs properties.
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public override string ToString() => DisplayName;
    }
}
