using System;
using Marple.Core;

namespace ChronoDigger.Combat {
    [Flags]
    public enum DamageFlags {
        None = 0,
        IgnoreBlock = 1 << 0,
        IgnoreResistances = 1 << 1,
    }

    /// <summary>
    /// For describing an instance of damage.
    /// </summary>
    public struct DamageInfo {
        public Amount value;
        public Actor source;
        public DamageType type;
        public DamageFlags flags;

        public void AddFlags(DamageFlags flagsN) {
            flags |= flagsN;
        }
        
        public void RemoveFlags(DamageFlags flagsN) {
            flags &= ~flagsN;
        }

        public bool HasFlags(DamageFlags flagsN) {
            return (flags & flagsN) == flagsN;
        }
    }
}