using System;
using Marple.Core;

namespace ChronoDigger.Combat {
    /// Serializble helper class to pair along-side a value to describe that value's damage properties.
    [Serializable]
    public class DamageDescriptor {
        public DamageType Type;
        public DamageFlags Flags;

        public DamageInfo DefineInfo(Amount value, Actor source) => new DamageInfo() {
            Value = value,
            Source = source,
            Type = Type,
            Flags = Flags,
        };
    }
}