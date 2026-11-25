using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Marple.Core;

namespace Marple.Core.Tests
{
    public class AmountTests
    {
        // ------------------------------------------------------------------
        // Creation
        // ------------------------------------------------------------------

        [Test]
        public void Constants_HaveExpectedValues()
        {
            Assert.IsTrue(Amount.Zero.IsZero);
            Assert.AreEqual(1d, Amount.One.ToDouble());
        }

        [Test]
        public void ImplicitConversion_FromDoubleAndInt()
        {
            Amount fromDouble = 2.5;
            Amount fromInt = 7;
            Assert.AreEqual(2.5d, fromDouble.ToDouble());
            Assert.AreEqual(7d, fromInt.ToDouble());
        }

        [Test]
        public void FromScientific_BuildsMantissaTimesPowerOfTen()
        {
            Assert.IsTrue(Amount.FromScientific(1.5, 40).ApproximatelyEquals(1.5e40));
            Assert.IsTrue(Amount.FromScientific(2, -3).ApproximatelyEquals(0.002));
        }

        [Test]
        public void Pow10_BuildsPowerOfTen()
        {
            Assert.AreEqual(1000d, Amount.Pow10(3).ToDouble());
            Assert.AreEqual(1d, Amount.Pow10(0).ToDouble());
        }

        // ------------------------------------------------------------------
        // Arithmetic
        // ------------------------------------------------------------------

        [Test]
        public void Add_Works() => Assert.AreEqual(4, (Amount.Wrap(1) + Amount.Wrap(3)).FloorToIntClamped());

        [Test]
        public void Subtract_Works() => Assert.AreEqual(Amount.Wrap(-2), Amount.Wrap(1) - Amount.Wrap(3));

        [Test]
        public void Multiply_Works() => Assert.AreEqual(Amount.Wrap(7.5), Amount.Wrap(5) * 1.5);

        [Test]
        public void Divide_Works() => Assert.AreEqual(Amount.Wrap(2.5), Amount.Wrap(5) / 2);

        [Test]
        public void Negate_Works() => Assert.AreEqual(Amount.Wrap(-5), -Amount.Wrap(5));

        [Test]
        public void NamedArithmetic_MatchesOperators()
        {
            Amount a = 12, b = 4;
            Assert.AreEqual(a + b, Amount.Add(a, b));
            Assert.AreEqual(a - b, Amount.Subtract(a, b));
            Assert.AreEqual(a * b, Amount.Multiply(a, b));
            Assert.AreEqual(a / b, Amount.Divide(a, b));
        }

        [Test]
        public void InstanceArithmetic_MatchesOperators()
        {
            Amount a = 12, b = 4;
            Assert.AreEqual(a + b, a.Add(b));
            Assert.AreEqual(a - b, a.Subtract(b));
            Assert.AreEqual(a * b, a.Multiply(b));
            Assert.AreEqual(a / b, a.Divide(b));
        }

        [Test]
        public void InstanceArithmetic_DoesNotMutateReceiver()
        {
            Amount a = 12;
            a.Add(4);
            Assert.AreEqual(Amount.Wrap(12), a);
        }

        [Test]
        public void Arithmetic_HandlesHugeValues()
        {
            Amount big = Amount.FromScientific(1, 300);
            Assert.IsTrue((big * 1000).ApproximatelyEquals(Amount.FromScientific(1, 303)));
        }

        // ------------------------------------------------------------------
        // Validation (only active in DEBUG / UNITY_ASSERTIONS builds)
        // ------------------------------------------------------------------

#if DEBUG || UNITY_ASSERTIONS
        [Test]
        public void DivideByZero_Throws() =>
            Assert.Throws<OverflowException>(() => { var _ = Amount.One / Amount.Zero; });

        [Test]
        public void ZeroDividedByZero_Throws() =>
            Assert.Throws<ArithmeticException>(() => { var _ = Amount.Zero / Amount.Zero; });

        [Test]
        public void SqrtOfNegative_Throws() =>
            Assert.Throws<ArithmeticException>(() => Amount.Sqrt(-4));

        [Test]
        public void Overflow_Throws() =>
            Assert.Throws<OverflowException>(() => { var _ = Amount.FromScientific(1, 308) * 100; });

        [Test]
        public void WrapNaN_Throws() =>
            Assert.Throws<ArithmeticException>(() => Amount.Wrap(double.NaN));

        [Test]
        public void WrapInfinity_Throws() =>
            Assert.Throws<OverflowException>(() => Amount.Wrap(double.PositiveInfinity));
#endif

        // ------------------------------------------------------------------
        // Comparison & equality
        // ------------------------------------------------------------------

        [Test]
        public void ComparisonOperators_Work()
        {
            Amount small = 1, large = 2, alsoSmall = 1;
            Assert.IsTrue(small < large);
            Assert.IsTrue(large > small);
            Assert.IsTrue(small <= alsoSmall);
            Assert.IsTrue(small >= alsoSmall);
            Assert.IsTrue(small != large);
            Assert.IsFalse(small == large);
        }

        [Test]
        public void Equals_AndHashCode_AgreeForEqualValues()
        {
            Amount a = 3.25, b = 3.25;
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a.Equals((object)b));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equals_Object_RejectsOtherTypes()
        {
            Assert.IsFalse(Amount.One.Equals((object)1d));
            Assert.IsFalse(Amount.One.Equals(null));
        }

        [Test]
        public void NegativeZero_EqualsZero_IncludingHashCode()
        {
            Amount negZero = -Amount.Zero;
            Assert.IsTrue(negZero == Amount.Zero);
            Assert.IsTrue(negZero.Equals(Amount.Zero));
            Assert.AreEqual(Amount.Zero.GetHashCode(), negZero.GetHashCode());
        }

        [Test]
        public void CompareTo_OrdersCorrectly()
        {
            Assert.Less(Amount.Wrap(1).CompareTo(Amount.Wrap(2)), 0);
            Assert.Greater(Amount.Wrap(2).CompareTo(Amount.Wrap(1)), 0);
            Assert.AreEqual(0, Amount.Wrap(2).CompareTo(Amount.Wrap(2)));
        }

        [Test]
        public void CompareTo_Object_HandlesNullAndWrongType()
        {
            Assert.AreEqual(1, Amount.One.CompareTo(null));
            Assert.AreEqual(0, Amount.One.CompareTo((object)Amount.One));
            Assert.Throws<ArgumentException>(() => Amount.One.CompareTo("1"));
        }

        [Test]
        public void ApproximatelyEquals_ToleratesFloatingPointDrift()
        {
            Amount drifted = Amount.Wrap(0.1) + Amount.Wrap(0.2);
            Assert.IsFalse(drifted == Amount.Wrap(0.3));
            Assert.IsTrue(drifted.ApproximatelyEquals(0.3));
        }

        [Test]
        public void ApproximatelyEquals_RejectsRealDifferences()
        {
            Assert.IsFalse(Amount.Wrap(100).ApproximatelyEquals(101));
            Assert.IsFalse(Amount.Zero.ApproximatelyEquals(1e-300));
        }

        [Test]
        public void ApproximatelyEquals_IsRelativeAtLargeScale()
        {
            Amount a = Amount.FromScientific(1, 100);
            Amount b = a * (1 + 1e-12);
            Assert.IsTrue(a.ApproximatelyEquals(b));
        }

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        [Test]
        public void SignQueries_Work()
        {
            Assert.IsTrue(Amount.Wrap(5).IsPositive);
            Assert.IsTrue(Amount.Wrap(-5).IsNegative);
            Assert.IsFalse(Amount.Zero.IsPositive);
            Assert.IsFalse(Amount.Zero.IsNegative);
            Assert.AreEqual(1, Amount.Wrap(5).Sign);
            Assert.AreEqual(-1, Amount.Wrap(-5).Sign);
            Assert.AreEqual(0, Amount.Zero.Sign);
        }

        [TestCase(1.5e40, 40, 1.5)]
        [TestCase(0.02, -2, 2.0)]
        [TestCase(1.0, 0, 1.0)]
        [TestCase(999.0, 2, 9.99)]
        [TestCase(-4500.0, 3, -4.5)]
        [TestCase(0.0, 0, 0.0)]
        public void ExponentAndMantissa_Decompose(double value, int exponent, double mantissa)
        {
            Amount a = value;
            Assert.AreEqual(exponent, a.Exponent);
            Assert.AreEqual(mantissa, a.Mantissa, 1e-9);
        }

        [Test]
        public void Mantissa_StaysInRange_ForEveryPowerOfTen()
        {
            for (int e = -300; e <= 300; e++)
            {
                Amount a = Amount.Pow10(e);
                Assert.AreEqual(e, a.Exponent, $"Exponent of 1e{e}");
                Assert.That(a.Mantissa, Is.GreaterThanOrEqualTo(1d).And.LessThan(10d), $"Mantissa of 1e{e}");
            }
        }

        // ------------------------------------------------------------------
        // Math
        // ------------------------------------------------------------------

        [Test]
        public void AbsMinMaxClamp_Work()
        {
            Assert.AreEqual(Amount.Wrap(3), Amount.Abs(-3));
            Assert.AreEqual(Amount.Wrap(2), Amount.Min(2, 5));
            Assert.AreEqual(Amount.Wrap(5), Amount.Max(2, 5));
            Assert.AreEqual(Amount.Wrap(10), Amount.Clamp(15, 0, 10));
            Assert.AreEqual(Amount.Wrap(0), Amount.Clamp(-5, 0, 10));
            Assert.AreEqual(Amount.Wrap(7), Amount.Clamp(7, 0, 10));
        }

        [TestCase(2.5, 2.0, 3.0, 3.0)]
        [TestCase(-2.5, -3.0, -2.0, -3.0)]
        [TestCase(2.4, 2.0, 3.0, 2.0)]
        [TestCase(4.0, 4.0, 4.0, 4.0)]
        public void FloorCeilingRound_Work(double value, double floor, double ceiling, double round)
        {
            Assert.AreEqual(floor, Amount.Floor(value).ToDouble());
            Assert.AreEqual(ceiling, Amount.Ceiling(value).ToDouble());
            Assert.AreEqual(round, Amount.Round(value).ToDouble(), "Round should be away from zero");
        }

        [Test]
        public void PowAndSqrt_Work()
        {
            Assert.AreEqual(Amount.Wrap(8), Amount.Pow(2, 3));
            Assert.AreEqual(Amount.Wrap(3), Amount.Sqrt(9));
            Assert.IsTrue(Amount.Pow(1.15, 10).ApproximatelyEquals(4.0455577357079067));
        }

        [Test]
        public void Logs_Work()
        {
            Assert.AreEqual(40d, Amount.Log10(Amount.Pow10(40)), 1e-12);
            Assert.AreEqual(1d, Amount.Ln(Math.E), 1e-12);
            Assert.AreEqual(10d, Amount.Log(1024, 2), 1e-12);
        }

        // ------------------------------------------------------------------
        // Exits
        // ------------------------------------------------------------------

        [TestCase(3.9, 3)]
        [TestCase(-3.1, -4)]
        [TestCase(0.0, 0)]
        [TestCase(1e20, int.MaxValue)]
        [TestCase(-1e20, int.MinValue)]
        [TestCase(2147483647.5, int.MaxValue)]
        public void FloorToIntClamped_FloorsAndClamps(double value, int expected) =>
            Assert.AreEqual(expected, Amount.Wrap(value).FloorToIntClamped());

        // ------------------------------------------------------------------
        // Display
        // ------------------------------------------------------------------

        [TestCase(0.0, "0")]
        [TestCase(12.0, "12")]
        [TestCase(3.5, "3.5")]
        [TestCase(3.14159, "3.14")]
        [TestCase(45210.0, "45,210")]
        [TestCase(45210.9, "45,210")]
        [TestCase(-45210.0, "-45,210")]
        [TestCase(999999.0, "999,999")]
        [TestCase(1e6, "1.00e6")]
        [TestCase(1.23456e45, "1.23e45")]
        [TestCase(-1.5e10, "-1.50e10")]
        public void Format_Defaults(double value, string expected) =>
            Assert.AreEqual(expected, Amount.Wrap(value).Format());

        [Test]
        public void Format_RenormalisesWhenMantissaRoundsUpToTen() =>
            Assert.AreEqual("1.00e7", Amount.Wrap(9.9996e6).Format());

        [Test]
        public void Format_RespectsSignificantDigitsAndThreshold()
        {
            Assert.AreEqual("1.2346e45", Amount.Wrap(1.23456e45).Format(significantDigits: 5));
            Assert.AreEqual("1e45", Amount.Wrap(1.23456e45).Format(significantDigits: 1));
            Assert.AreEqual("1.50e3", Amount.Wrap(1500).Format(scientificThreshold: 1000));
        }

        [Test]
        public void Format_NeverShowsNegativeZero()
        {
            Assert.AreEqual("0", (-Amount.Zero).Format());
            Assert.AreEqual("0", Amount.Round(-0.4).Format());
        }

        [Test]
        public void Format_IgnoresCurrentCulture()
        {
            CultureInfo previous = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                Assert.AreEqual("3.5", Amount.Wrap(3.5).Format());
                Assert.AreEqual("45,210", Amount.Wrap(45210).Format());
                Assert.AreEqual("1.23e45", Amount.Wrap(1.23456e45).Format());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }

        [Test]
        public void ToString_MatchesFormat() =>
            Assert.AreEqual(Amount.Wrap(1.23456e45).Format(), Amount.Wrap(1.23456e45).ToString());

        // ------------------------------------------------------------------
        // Saving & loading
        // ------------------------------------------------------------------

        [TestCase(0.0)]
        [TestCase(1.0)]
        [TestCase(-42.125)]
        [TestCase(0.30000000000000004)]
        [TestCase(1.2345678901234567e300)]
        [TestCase(5e-324)]
        public void SaveString_RoundTripsExactly(double value)
        {
            Amount original = value;
            Amount loaded = Amount.Parse(original.ToSaveString());
            Assert.AreEqual(original, loaded);
        }

        [Test]
        public void SaveString_IgnoresCurrentCulture()
        {
            CultureInfo previous = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                Assert.AreEqual("1.5", Amount.Wrap(1.5).ToSaveString());
                Assert.AreEqual(Amount.Wrap(1.5), Amount.Parse("1.5"));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }

        [Test]
        public void TryParse_AcceptsValidInput()
        {
            Assert.IsTrue(Amount.TryParse("1.5e40", out Amount result));
            Assert.AreEqual(Amount.FromScientific(1.5, 40), result);
        }

        [TestCase("")]
        [TestCase("abc")]
        [TestCase("1,5")]
        [TestCase("NaN")]
        [TestCase("Infinity")]
        [TestCase("1e400")]
        public void TryParse_RejectsInvalidInput(string input)
        {
            Assert.IsFalse(Amount.TryParse(input, out Amount result));
            Assert.AreEqual(Amount.Zero, result);
        }

        [Test]
        public void Parse_ThrowsOnGarbage() =>
            Assert.Throws<FormatException>(() => Amount.Parse("abc"));
    }
}
