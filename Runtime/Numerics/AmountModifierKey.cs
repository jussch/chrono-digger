using System;

namespace Marple.Core {
    /// <summary>
    /// Identifies a modifier inside an <see cref="AmountModifierRecord"/>: the object that
    /// applied it, plus an index so one source can apply several modifiers.
    /// Sources are compared with <see cref="object.Equals(object, object)"/>.
    /// </summary>
    [Serializable]
    public readonly struct AmountModifierKey : IEquatable<AmountModifierKey> {
        public readonly object Source;
        public readonly int Index;

        public AmountModifierKey(object source, int index = 0) {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Index = index;
        }

        public bool Equals(AmountModifierKey other) => Index == other.Index && Equals(Source, other.Source);
        public override bool Equals(object obj) => obj is AmountModifierKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Source, Index);

        public static bool operator ==(AmountModifierKey left, AmountModifierKey right) => left.Equals(right);
        public static bool operator !=(AmountModifierKey left, AmountModifierKey right) => !left.Equals(right);

        public override string ToString() => $"{Source}#{Index}";
    }
}
