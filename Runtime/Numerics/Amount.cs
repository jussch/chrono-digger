using System;
using System.Diagnostics;
using System.Globalization;

namespace Marple.Core
{
    /// <summary>
    /// A number for the incremental economy: costs, rates, stockpiles.
    ///
    /// Currently backed by a double (max ~1.8e308). The backing value is private and
    /// nothing outside this struct may touch it, so Amount can later be backed by
    /// BigDouble by changing only this file.
    ///
    /// Not for combat. Combat uses plain ints/doubles and rounds once at the end.
    /// </summary>
    public readonly struct Amount : IEquatable<Amount>, IComparable<Amount>, IComparable
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        private readonly double _value;

        private Amount(double value)
        {
            Validate(value);
            _value = value;
        }

        // ------------------------------------------------------------------
        // Constants & creation
        // ------------------------------------------------------------------

        public static readonly Amount Zero = new Amount(0d);
        public static readonly Amount One = new Amount(1d);

        /// <summary>Lets literals and ints flow in naturally: <c>Amount gold = 50;</c>, <c>cost * 1.5</c>.</summary>
        public static implicit operator Amount(double value) => new Amount(value);

        public static Amount Wrap(double value) => new Amount(value);
        public static Amount Wrap(int value) => new Amount(value);

        /// <summary>mantissa × 10^exponent, e.g. FromScientific(1.5, 40) = 1.5e40.</summary>
        public static Amount FromScientific(double mantissa, int exponent) =>
            new Amount(mantissa * Math.Pow(10d, exponent));

        /// <summary>10^exponent.</summary>
        public static Amount Pow10(int exponent) => new Amount(Math.Pow(10d, exponent));

        // ------------------------------------------------------------------
        // Arithmetic
        // ------------------------------------------------------------------

        public static Amount operator +(Amount a, Amount b) => new Amount(a._value + b._value);
        public static Amount operator -(Amount a, Amount b) => new Amount(a._value - b._value);
        public static Amount operator *(Amount a, Amount b) => new Amount(a._value * b._value);
        public static Amount operator /(Amount a, Amount b) => new Amount(a._value / b._value);
        public static Amount operator -(Amount a) => new Amount(-a._value);

        public static Amount Add(Amount a, Amount b) => a + b;
        public static Amount Subtract(Amount a, Amount b) => a - b;
        public static Amount Multiply(Amount a, Amount b) => a * b;
        public static Amount Divide(Amount a, Amount b) => a / b;

        public Amount Add(Amount other) => this + other;
        public Amount Subtract(Amount other) => this - other;
        public Amount Multiply(Amount other) => this * other;
        public Amount Divide(Amount other) => this / other;

        // ------------------------------------------------------------------
        // Comparison & equality
        // ------------------------------------------------------------------

        public static bool operator ==(Amount a, Amount b) => a._value == b._value;
        public static bool operator !=(Amount a, Amount b) => a._value != b._value;
        public static bool operator <(Amount a, Amount b) => a._value < b._value;
        public static bool operator >(Amount a, Amount b) => a._value > b._value;
        public static bool operator <=(Amount a, Amount b) => a._value <= b._value;
        public static bool operator >=(Amount a, Amount b) => a._value >= b._value;

        public bool Equals(Amount other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is Amount other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();

        public int CompareTo(Amount other) => _value.CompareTo(other._value);

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;
            if (obj is Amount other) return CompareTo(other);
            throw new ArgumentException($"Object must be of type {nameof(Amount)}.", nameof(obj));
        }

        /// <summary>
        /// Equality within a relative tolerance. Use this instead of == for anything
        /// that went through multiplication/division (e.g. "can I afford this?" checks
        /// on values that should be equal but drifted by a rounding error).
        /// </summary>
        public bool ApproximatelyEquals(Amount other, double relativeTolerance = 1e-9)
        {
            if (_value == other._value) return true;
            double scale = Math.Max(Math.Abs(_value), Math.Abs(other._value));
            return Math.Abs(_value - other._value) <= scale * relativeTolerance;
        }

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        public bool IsZero => _value == 0d;
        public bool IsPositive => _value > 0d;
        public bool IsNegative => _value < 0d;

        /// <summary>-1, 0 or 1.</summary>
        public int Sign => Math.Sign(_value);

        /// <summary>Power of ten: 1.5e40 → 40, 0.02 → -2, 0 → 0.</summary>
        public int Exponent => _value == 0d ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(_value)));

        /// <summary>Leading digits: 1.5e40 → 1.5. Always in [1, 10) for non-zero values (sign preserved).</summary>
        public double Mantissa => _value == 0d ? 0d : _value / Math.Pow(10d, Exponent);

        // ------------------------------------------------------------------
        // Math
        // ------------------------------------------------------------------

        public static Amount Abs(Amount a) => new Amount(Math.Abs(a._value));
        public static Amount Min(Amount a, Amount b) => a <= b ? a : b;
        public static Amount Max(Amount a, Amount b) => a >= b ? a : b;
        public static Amount Clamp(Amount value, Amount min, Amount max) => Max(min, Min(max, value));

        public static Amount Floor(Amount a) => new Amount(Math.Floor(a._value));
        public static Amount Ceiling(Amount a) => new Amount(Math.Ceiling(a._value));
        public static Amount Round(Amount a) => new Amount(Math.Round(a._value, MidpointRounding.AwayFromZero));

        public static Amount Pow(Amount value, double exponent) => new Amount(Math.Pow(value._value, exponent));
        public static Amount Sqrt(Amount a) => new Amount(Math.Sqrt(a._value));

        // Logs return double on purpose: the log of even an enormous number is small,
        // and BigDouble's logs return double too, so these stay stable through a port.
        public static double Log10(Amount a) => Math.Log10(a._value);
        public static double Ln(Amount a) => Math.Log(a._value);
        public static double Log(Amount a, double newBase) => Math.Log(a._value, newBase);

        // ------------------------------------------------------------------
        // Leaving Amount (the narrow, deliberate exits)
        // ------------------------------------------------------------------

        /// <summary>
        /// Floors to an int, clamped to int range. For bridging into systems that
        /// genuinely count things (e.g. "how many purchases can I afford").
        /// </summary>
        public int FloorToIntClamped()
        {
            double floored = Math.Floor(_value);
            if (floored >= int.MaxValue) return int.MaxValue;
            if (floored <= int.MinValue) return int.MinValue;
            return (int)floored;
        }

        /// <summary>
        /// Raw double. Only for values known to be small, like a progress-bar ratio
        /// (current / target). The awkward name is deliberate: every call site is a
        /// place a BigDouble port would have to check.
        /// </summary>
        public double ToDoubleLossy() => _value;

        // ------------------------------------------------------------------
        // Display
        // ------------------------------------------------------------------

        public override string ToString() => Format();

        /// <summary>
        /// Player-facing text. Below the threshold: "12", "3.5", "45,210".
        /// At or above it: scientific, e.g. "1.23e45".
        /// </summary>
        public string Format(int significantDigits = 3, double scientificThreshold = 1e6)
        {
            // Catches -0 too, which would otherwise print as "-0".
            if (_value == 0d) return "0";

            double abs = Math.Abs(_value);

            if (abs < scientificThreshold)
            {
                if (abs >= 1000d) return Math.Truncate(_value).ToString("N0", Culture);
                return _value.ToString("0.##", Culture);
            }

            int decimals = Math.Max(0, significantDigits - 1);
            int exponent = Exponent;
            double mantissa = Math.Round(_value / Math.Pow(10d, exponent), decimals);

            // Rounding can push 9.9996 up to 10.00, so renormalise.
            if (Math.Abs(mantissa) >= 10d)
            {
                mantissa /= 10d;
                exponent++;
            }

            return mantissa.ToString("F" + decimals, Culture) + "e" + exponent.ToString(Culture);
        }

        // ------------------------------------------------------------------
        // Saving & loading
        // ------------------------------------------------------------------

        // Saved as a string, not a double, so the save format survives a BigDouble port.

        public string ToSaveString() => _value.ToString("R", Culture);

        public static Amount Parse(string s) =>
            new Amount(double.Parse(s, NumberStyles.Float, Culture));

        public static bool TryParse(string s, out Amount result)
        {
            if (double.TryParse(s, NumberStyles.Float, Culture, out double parsed) && IsValid(parsed))
            {
                result = new Amount(parsed);
                return true;
            }

            result = Zero;
            return false;
        }

        // ------------------------------------------------------------------
        // Validation
        // ------------------------------------------------------------------

        private static bool IsValid(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        // Stripped from release builds. Multiple [Conditional]s are OR'd:
        // runs in .NET debug builds and in the Unity editor / development builds.
        [Conditional("DEBUG"), Conditional("UNITY_ASSERTIONS")]
        private static void Validate(double value)
        {
            if (double.IsNaN(value))
                throw new ArithmeticException("Amount became NaN (e.g. 0/0, Sqrt of a negative).");

            if (double.IsInfinity(value))
                throw new OverflowException(
                    "Amount went past the double range (~1.8e308) or divided by zero. " +
                    "If this is real growth, it's time to back Amount with BigDouble.");
        }
    }
}


// FOR Newtonsoft.Json LATER
/*
using System;
using Newtonsoft.Json;

namespace Marple.Core
{
    public sealed class AmountJsonConverter : JsonConverter<Amount>
    {
        public override void WriteJson(JsonWriter writer, Amount value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToSaveString());

        public override Amount ReadJson(JsonReader reader, Type objectType, Amount existingValue,
                                        bool hasExistingValue, JsonSerializer serializer) =>
            Amount.Parse(reader.Value?.ToString() ?? "0");
    }
}
*/