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
        public Amount Value;
        public Actor Source;
        public DamageType Type;
        public DamageFlags Flags;

        public void AddFlags(DamageFlags flagsN) {
            Flags |= flagsN;
        }
        
        public void RemoveFlags(DamageFlags flagsN) {
            Flags &= ~flagsN;
        }

        public bool HasFlags(DamageFlags flagsN) {
            return (Flags & flagsN) == flagsN;
        }
    }
}