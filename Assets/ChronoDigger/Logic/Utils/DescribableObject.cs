using Marple.Core;

namespace ChronoDigger.Utils {
    public abstract class DescribableObject : IdScriptableObject, IDescribable {
        // [field: SerializeField] on these two in Unity: they're auto-properties because IDescribable needs properties.
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public override string ToString() => DisplayName;
    }
}