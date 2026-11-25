using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace Marple.Core {
        
    public static class Utils { 
        private static readonly Random Rng = new Random();

        private static readonly List<string> CompactCategories = new List<string> {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", 
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
        };
        private static readonly List<string> NumberCategories = new List<string> {
            "K",
            "M",
            "B",
            "T",
            "Qa",
            "Qt",
            "Sx",
            "Sp",
            "Oc",
            "No",
            "Dc",
            "UDc",
            "DDc",
            "TDc",
            "QaDc",
            "QiDc",
            "SxDc",
            "SpDc",
            "OcDc",
            "NoDc",
            "Vg",
            "UVg",
            "DVg",
            "TVg",
            "QaVg",
            "QiVg",
            "SxVg",
            "SpVg",
            "OcVg",
            "NoVg",
            "Tr",
            "UTr",
            "DTr",
            "TTr",
            "QaTr",
            "QiTr",
            "SxTr",
            "SpTr",
            "OcTr",
            "NoTr",
            "DcTr",
        };
        private static readonly List<string> ReverseNumberCategories = new List<string> {
            "m", "u", "n", "p", "f", "a", "z", "y", "r", "q",
        };

        public enum BeautifyType {
            Named,
            Compact,
            CompactNum,
            Scientific,
            Engineering
        }
        public struct BeautifySettings {
            public Amount accuracy;
            public bool space;
            public BeautifyType type;

            public static BeautifySettings Default = new BeautifySettings() {
                accuracy = 0,
                space = true,
                type = BeautifyType.Named,
            };

            public static BeautifySettings WithSpace(bool space) {
                var settings = Default;
                settings.space = space;
                return settings;
            }
        }

        public static string BeautifyInt(Amount number) => BeautifyInt(number, BeautifySettings.Default);
        public static string BeautifyInt(Amount number, BeautifySettings settings) {
            return number > 100000 ? BeautifyNumber(number, settings) : number.ToInt().ToString();
        }

        public static string BeautifyNumber(Amount number) => BeautifyNumber(number, BeautifySettings.Default);
        public static string BeautifyNumber(Amount number, BeautifySettings settings) {
            if (number < 0) return "-" + BeautifyNumber(number.Abs(), settings);
            if (number == 0) return "0";
            if (settings.accuracy != 0 &&
                number > settings.accuracy && 
                number < (settings.accuracy + 0.1) && 
                HasDecimals(number.ToDouble() * 100)) {
                    return ToDecimalPlace(number.ToDouble(), 3);         
            }
            return BeautifyNumberSwitch(number, settings);
        }

        private static string BeautifyNumberSwitch(Amount number, BeautifySettings settings) {
            switch (settings.type) {
                case BeautifyType.Named: return BeautifyNumberNamed(number, settings);
                case BeautifyType.Compact: return BeautifyNumberCompact(number, settings);
                case BeautifyType.CompactNum: return BeautifyNumberCompactNumerical(number, settings);
                case BeautifyType.Scientific: return BeautifyNumberScientific(number, settings);
                case BeautifyType.Engineering: return BeautifyNumberEngineering(number, settings);
                default: throw new ArgumentOutOfRangeException("Invalid NumberDisplayOption: " + settings.type);
            }
        }
        
        public static string BeautifyNumberNamed(Amount number, BeautifySettings settings) {
            var (roundedString, categoryIndex, inverted) = BeautifyNumberThirds(number);

            // var categoryArray = inverted ? ReverseNumberCategories : NumberCategories;
            var categoryArray = NumberCategories;
            
            if (categoryIndex < 0) return roundedString;
            if (categoryIndex >= categoryArray.Count) return roundedString + "e" + (inverted ? "-" : "") + ((categoryIndex + 1) * 3);
            
            string symbol = categoryArray.ElementAt((int)categoryIndex);
            return roundedString + (settings.space ? "<space=0.1em>" : "") + (inverted ? "-" : "") + symbol;
        }
    
        public static string BeautifyNumberCompact(Amount number, BeautifySettings settings) {
            var (roundedString, categoryIndex, inverted) = BeautifyNumberThirds(number);
            
            if (categoryIndex < 0) return roundedString;

            string symbol = GetCompactLetter(categoryIndex + 1);

            return roundedString + (settings.space ? "<space=0.1em>" : "") + (inverted ? "-" : "") + symbol;
        }
    
        public static string BeautifyNumberCompactNumerical(Amount number, BeautifySettings settings) {
            var (roundedString, categoryIndex, inverted) = BeautifyNumberThirds(number);
            
            if (categoryIndex < 0) return roundedString;

            long val = (categoryIndex + 1) % 100;
            long letterIndex = ((categoryIndex + 1) / 100) + 1;
            string symbol = $"{GetCompactLetter(letterIndex).ToLower()}{val}";

            return roundedString + (settings.space ? "<space=0.1em>" : "") + (inverted ? "-" : "") + symbol;
        }

        public static (string, long, bool) BeautifyNumberThirds(Amount number) {
            bool hasDecimals = HasDecimalsOld(number.ToDouble());
            bool inverted = false;
            
            if (number >= 0.01) {
                if (number < 0.1 && hasDecimals) return (ToDecimalPlace(number, 3), -1, false);
                if (number < 10 && hasDecimals) return (ToDecimalPlace(number, 2), -1, false);
                if (number < 100 && hasDecimals) return (ToDecimalPlace(number, 1), -1, false);
            } else if (hasDecimals) {
                // Invert this number...
                inverted = true;
            }
            
            long numDigits = GetNumDigits(number);
            long thirds = (long)Math.Floor((numDigits - 1) / 3f);
            long categoryIndex = Math.Abs(thirds) - 1;
            if (categoryIndex < 0) return (number.Round().ToString(), -1, inverted);
            
            long digitsRemoved = thirds * 3;
            long digitsRemaining = numDigits - digitsRemoved;
            Amount valueRemoved = Amount.Pow(10, digitsRemoved);
            Amount precisionMod = Amount.Pow(10, (3 - digitsRemaining));
            Amount roundedNumber = (number / valueRemoved * precisionMod).Floor() / precisionMod;

            // string format = "0.".PadRight((int)(5 - digitsRemaining), '0');
            string roundedString = roundedNumber.ToString("G3");
            bool hasDecimal = roundedString.Contains(".");
            if (roundedString.Length < 4 && hasDecimal) roundedString = (roundedString).PadRight(4, '0');
            if (roundedString.Length < 3 && !hasDecimal) roundedString = (roundedString + ".").PadRight(4, '0');
            // if (roundedString.Length < 3 && digitsRemaining == 3) roundedString = (roundedString).PadRight(4, '0');

            return (roundedString, categoryIndex, inverted);
        }

        private static string BeautifyNumberSimplify(Amount number) {
            bool hasDecimals = HasDecimalsOld(number.ToDouble());
            if (!hasDecimals) return number.Round().ToString();
            if (number < 1) return ToDecimalPlace(number, 3);
            if (number < 10) return ToDecimalPlace(number, 2);
            if (number < 100) return ToDecimalPlace(number, 1);
            return ToDecimalPlace(number, 0);
        }

        private static string BeautifyNumberScientific(Amount number, BeautifySettings settings) {
            if (number < 1000 && number >= 0.1) {
                return BeautifyNumberSimplify(number);
            }
            
            return number.Mantissa.ToString("F2") + "e" + number.Exponent;
        }

        private static string BeautifyNumberEngineering(Amount number, BeautifySettings settings) {
            if (number < 1000 && number >= 0.1) {
                return BeautifyNumberSimplify(number);
            }
            
            var (roundedString, categoryIndex, inverted) = BeautifyNumberThirds(number);
            return roundedString + "e" + (inverted ? "-" : "") + ((categoryIndex + 1) * 3);
        }

        public static string BeautifyNumberSigned(Amount number, BeautifySettings settings) {
            return (number > 0 ? "+" : "") + BeautifyNumber(number, settings);
        }

        public static string BeautifyPercent(Amount number, BeautifySettings settings) {
            return "x" + SmallSpace + BeautifyNumber(number.AsPercent(), settings);
        }

        public static string BeautifyRate(Amount rate) {
            return (rate >= 0 ? "+" : "") + BeautifyNumber(rate);
        }

        public static string BeautifyRatePerSec(Amount rate) {
            if (rate >= 0.1) return FractionStr(BeautifyRate(rate), "s");
            if (rate >= 0.001) return FractionStr(BeautifyRate(rate * 60), "m");
            return FractionStr(BeautifyRate(rate * 3600), "h");
        }

        public static string GetSign(Amount val) {
            return val == 0 ? "" : (val > 0 ? "+" : "-");
        }

        public const string SmallSpace = "<space=0.1em>";
        private const double YearSeconds = 60 * 60 * 24 * 365.25;
        private const double DaySeconds = 60 * 60 * 24;
        private const double HourSeconds = 60 * 60;
        private const double MinuteSeconds = 60;
        
        public static string BeautifyTime(Amount seconds) {
            if (seconds < 0) return "-" + BeautifyTime(Amount.Abs(seconds));
            if (seconds < 1) return seconds.ToString("F1");
            if (seconds < 99) return seconds.ToString("F0");
            if (seconds < MinuteSeconds * 99) return (seconds / MinuteSeconds).ToString("F0") + "m";
            if (seconds < HourSeconds * 99) return (seconds / HourSeconds).ToString("F0") + "h";
            if (seconds < DaySeconds * 99) return (seconds / DaySeconds).ToString("F0") + "d";
            return BeautifyNumber(seconds / YearSeconds) + "y";
        }
        
        public static string BeautifyTimeAccurate(Amount seconds) {
            if (seconds < 0) return "-" + BeautifyTimeAccurate(Amount.Abs(seconds));
            if (seconds < 10) return seconds.ToString("F2") + "s";
            if (seconds < 100) return seconds.ToString("F1") + "s";
            if (seconds < 1000) return seconds.ToString("F0") + "s";
            if (seconds < MinuteSeconds * 1000) return (seconds / MinuteSeconds).ToString("F0") + "m";
            if (seconds < HourSeconds * 1000) return (seconds / HourSeconds).ToString("F0") + "h";
            if (seconds < DaySeconds * 1000) return (seconds / DaySeconds).ToString("F0") + "d";
            return BeautifyNumber(seconds / YearSeconds) + "y";
        }

        public static string BeautifyTimeUltraPrecise(Amount seconds) {
            if (seconds < 0) return "-" + BeautifyTimeUltraPrecise(Amount.Abs(seconds));
            if (seconds < 60) return Utils.BeautifyNumber(seconds) + "s";
            if (seconds > YearSeconds) return Utils.BeautifyNumber(seconds / YearSeconds) + "y";
            var secSlice = Amount.Mod(seconds, 60).Floor().ToInt();
            var minuteSlice = Amount.Mod(seconds / MinuteSeconds, 60).Floor().ToInt();
            var hourSlice = Amount.Floor(seconds / HourSeconds).Floor().ToInt();
            var list = new List<int>() { hourSlice, minuteSlice, secSlice };
            return string.Join(":", list.SkipWhile(val => val < 1).Select(val => val.ToString().PadLeft(2, '0')));
        }

        public static string BeautifyTimeUltraPreciseWithYears(Amount seconds) {
            if (seconds < 0) return "-" + BeautifyTimeUltraPreciseWithYears(Amount.Abs(seconds));
            if (seconds < 60) return Utils.BeautifyNumber(seconds) + "s";
            if (seconds > YearSeconds * 100) return Utils.BeautifyNumber(seconds / YearSeconds) + "y";
            var secSlice = Amount.Mod(seconds, 60).Floor().ToInt();
            var minuteSlice = Amount.Mod(seconds / MinuteSeconds, 60).Floor().ToInt();
            var hourSlice = Amount.Mod(seconds / HourSeconds, 24).Floor().ToInt();
            var daySlice = Amount.Mod(seconds / DaySeconds, 365.25).Floor().ToInt();
            var yearSlice = Amount.Floor(seconds / YearSeconds).ToInt();
            var list = new List<int>() { yearSlice, daySlice, hourSlice, minuteSlice, secSlice };
            return string.Join(":", list.SkipWhile(val => val < 1).Select(val => val.ToString().PadLeft(2, '0')));
        }

        private static readonly List<string> QuickRoman = new List<string>
            { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
        public static string BeautifyRoman(int value) {
            if (value >= 0 && value < QuickRoman.Count) return QuickRoman[value];
            return ToRoman(value);
        }
        
        private static string ToRoman(int number) {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1) return string.Empty;            
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900); 
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);            
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }

        public static bool HasDecimals(double val) {
            return Math.Abs(Math.Round(val) - val) > 0.001;
        }

        public static bool HasDecimalsOld(double val) {
            return (int)Math.Floor(val) != (int)Math.Ceiling(val);
        }

        public static string ToDecimalPlace(Amount val, int decimals) => ToDecimalPlace(val.ToDouble(), decimals);
        public static string ToDecimalPlace(double val, int decimals) {
            double rounded = Math.Round(val, decimals);
            string format = "0.".PadRight(2 + decimals, '0');
            return rounded.ToString(format);
        }

        // public static string Colorize(string text, Color color) {
        //     return "<#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
        // }

        // public static string ParseDescription(string text) {
        //     return text
        //         .Replace("<lore>", "<#cdeae7B4><size=80%>")
        //         .Replace("</lore>", "</size></color>")
        //         .Replace("<emph>", "<#97cbdb>")
        //         .Replace("</emph>", "</color>")
        //         .Replace("<emph2>", "<#ef86cf>")
        //         .Replace("</emph2>", "</color>")
        //         .Replace("<elemental>", $"<#{ColorUtility.ToHtmlStringRGBA(GlobalVariables.GetElementalColor())}>")
        //         .Replace("</elemental>", "</color>")
        //         .Replace("<positive>", "<#" + ColorUtility.ToHtmlStringRGBA(GlobalVariables.GetPositiveColor()) +">")
        //         .Replace("</positive>", "</color>")
        //         .Replace("<deemph>", "<#" + ColorUtility.ToHtmlStringRGBA(GlobalVariables.GetSubColor()) +">")
        //         .Replace("</deemph>", "</color>")
        //         .Replace("<negative>", "<#" + ColorUtility.ToHtmlStringRGBA(GlobalVariables.GetNegativeColor()) +">")
        //         .Replace("</negative>", "</color>")
        //         .Replace("<warn>", "<#" + ColorUtility.ToHtmlStringRGBA(GlobalVariables.GetWarningColor()) +">")
        //         .Replace("</warn>", "</color>")
        //         .Replace("<h1>", "<size=150%><#f4d782>")
        //         .Replace("</h1>", "</color></size>")
        //         .Replace("<h2>", "<size=120%><#dca658>")
        //         .Replace("</h2>", "</color></size>");
        // }

        // public static string CreateTextIcon(ITooltipable item) => CreateTextIcon(item, item.GetColor());
        // public static string CreateTextIcon(ITooltipable item, Color color) {
        //     return $"<sprite name=\"{item.GetIcon().name}\" color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        // }

        // public static string CreateTextIconStr(ITooltipable item, string str) => CreateTextIconStr(item, str, item.GetColor());
        // public static string CreateTextIconStr(ITooltipable item, string str, Color color) {
        //     return $"{CreateTextIcon(item, color)}<#{ColorUtility.ToHtmlStringRGBA(color)}>{str}</color>";
        // }

        public static string FractionStr(int frac1, int frac2, bool space = true) => FractionStr(frac1.ToString(), frac2.ToString(), space);
        public static string FractionStr(Amount frac1, Amount frac2, bool space = true) => FractionStr(BeautifyNumber(frac1, BeautifySettings.WithSpace(space)), BeautifyNumber(frac2, BeautifySettings.WithSpace(space)), space);
        public static string FractionStr(string frac1, string frac2, bool space = true) {
            string spaceType = space ? SmallSpace : "";
            return frac1 + spaceType + "/" + spaceType + frac2;
        }

        private const double MaxTimePer = 0.25;
        public static string FractionTimeStr(Amount time, Amount max) {
            return max < MaxTimePer ? FractionStr(BeautifyNumber(1 / max), "s") : FractionStr(BeautifyTimeAccurate(time), BeautifyTimeAccurate(max));
        }
        
        public static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        
        public static string Base64Decode(string base64EncodedData) {
            try {
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            } catch (FormatException) {
                return "";
            }
        }

        public static string GenExportString(string compType, string compName, string propName) {
            return $"({compType}) {compName} | {propName}";
        }

        private static string GetCompactLetter(long index) {
            string symbol = "";
            if (index <= 0) return symbol;
            
            int compactSize = CompactCategories.Count;
            int maxIterations = 50;
            long currIndex = index;
            while (currIndex >= 1 && maxIterations > 0) {
                maxIterations -= 1;
                int currDigit = (int)((currIndex - 1) % compactSize);
                currIndex = (currIndex - currDigit) / compactSize;
                symbol = CompactCategories[currDigit] + symbol;
            }

            return symbol;
        }
        
        // /**
        //  * GameObject
        //  */
        // public static void DestroyChildren(GameObject obj) {
        //     foreach (Transform child in obj.transform) {
        //         Object.Destroy(child.gameObject);
        //     }
        // }
        
        // public static void DestroyChildrenImmediate(GameObject obj, bool allowDeleteAssets = false) {
        //     foreach (Transform child in obj.transform) {
        //         Object.DestroyImmediate(child.gameObject, allowDeleteAssets);
        //     }
        // }

        // public static List<T> GetParentElements<T>(MonoBehaviour obj) where T : MonoBehaviour {
        //     return obj.GetComponentsInParent<T>().ToList();
        // }

        // public static List<Canvas> GetParentCanvases(MonoBehaviour obj) {
        //     return obj.GetComponentsInParent<Canvas>().ToList();
        // }
        
        // public static T FindParentComponent<T>(MonoBehaviour childObject) where T : MonoBehaviour {
        //     Transform t = childObject.transform;
        //     while (t.parent != null) {
        //         var comp = t.parent.GetComponent<T>();
        //         if (comp) {
        //             return comp;
        //         }
        //         t = t.parent.transform;
        //     }
        //     return null; // Could not find a parent with given tag.
        // }

        // /**
        //  * RectTransform
        //  */
        // public static void KeepRectOnScreenLocal(RectTransform rectTransform) {
        //     // Invert anchoring and position if dropdown is partially or fully outside of canvas rect.
        //     // Typically this will have the effect of placing the dropdown above the button instead of below,
        //     // but it works as inversion regardless of initial setup.
        //     Vector3[] corners = new Vector3[4];
        //     rectTransform.GetWorldCorners(corners);

        //     RectTransform rootCanvasRectTransform = GlobalVariables.GetCanvas().transform as RectTransform;
        //     Rect rootCanvasRect = rootCanvasRectTransform.rect;
        //     for (int axis = 0; axis < 2; axis++) {
        //         bool outside = false;
        //         for (int i = 0; i < 4; i++) {
        //             Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
        //             if ((corner[axis] < rootCanvasRect.min[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.min[axis])) ||
        //                 (corner[axis] > rootCanvasRect.max[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.max[axis])))
        //             {
        //                 outside = true;
        //                 break;
        //             }
        //         }
                
        //         if (outside) RectTransformUtility.FlipLayoutOnAxis(rectTransform, axis, false, false);
        //     }
        // }

        // public static Vector2 GetPointInRectangle(RectTransform rectTransform, Vector2 screenPosition) {
        //     Vector2 localPoint = new Vector2();
        //     RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, null, out localPoint);
        //     return localPoint;
        // }

        /**
        * Math
        */
        public static long GetNumDigits(Amount value) => value.Exponent + 1;
        public static long GetNumDigits(double value) {
            if (value < 0) return 0;
            return (long)Math.Floor(Math.Log10(value)) + 1;
        }

        public static long GetNumDigitsAtBase(Amount value, long baseVal) {
            if (value < 0) return 0;
            return (long)Math.Floor(Amount.Log(value, baseVal)) + 1;
        }

        public static long GetNumDigitsAtBase(Amount value, Amount baseVal) {
            if (value < 0) return 0;
            return (long)Math.Floor(Amount.Log(value, baseVal)) + 1;
        }
        
        public static Amount Mod(Amount value, Amount mod) {
            return value * (mod + 1);
        }
        
        public static Amount ModAtLevel(Amount value, Amount mod, int level) {
            return value * Amount.Pow(mod + 1, level - 1);
        }

        public static Amount PercentReduction(Amount value, Amount perc) {
            return value * (100 - perc) / 100;
        }

        public static Amount RoundToNearest(Amount value, Amount nearest) {
            return Amount.Floor(value / nearest) * nearest;
        }

        public static Amount RoundToTwoSigs(Amount value) {
            long digits = GetNumDigits(value);
            if (digits < 2) return value;
            Amount tens = Amount.Pow(10, digits - 2);
            return Amount.Round(value / tens) * tens;
        }

        public static Amount RoundToThreeSigs(Amount value) {
            long digits = GetNumDigits(value);
            Amount tens = Amount.Pow(10, digits - 3);
            return Amount.Round(value / tens) * tens;
        }

        public static Amount RoundToTwoSigsEarly(Amount value) {
            return value >= 100 ? value : Utils.RoundToTwoSigs(value);
        }

        public static int SnapFloorToInt(Amount value) => value.Add(0.001f).FloorToIntClamped();

        public static bool Approx(double val1, double val2) {
            return Math.Abs(val1 - val2) < 0.00001;
        }

        public static bool Approx(Amount val1, Amount val2) {
            return val1.Subtract(val2).Abs() < 0.00001;
        }

        public static bool Approx(Amount val1, Amount val2, Amount margin) {
            return val1.Subtract(val2).Abs() < margin;
        }

        public static bool ApproxSig(Amount val1, Amount val2) {
            if (val1 == 0 && val2 == 0) return true;
            if (val2 == 0 && val1 != 0) return false;
            return Amount.Abs((val1 - val2) / val2) < 0.01;
        }

        public static bool ApproxError(Amount val1, Amount val2) {
            return IsWithinError(val1, val2, 0.001);
        }

        public static bool IsWithinError(Amount val1, Amount val2, Amount error) {
            if (val1 == 0 && val2 == 0) return true;
            return Amount.Abs((val1 - val2) / (val2 == 0 ? val1 : val2)) < error;
        }

        public static double ChangeRatio(double startValue, double endValue, double toRatio, double fromRatio = 2) {
            return startValue * Math.Pow(toRatio, GetMagnitude(startValue, endValue, fromRatio));
        }
        
        public static Amount ChangeRatio(Amount startValue, Amount endValue, Amount toRatio, double fromRatio = 2) {
            return startValue * toRatio.Pow(GetMagnitude(startValue, endValue, fromRatio));
        }

        public static double ChangeRatio(double startValue, double endValue, AmountModifier toRatio, double fromRatio = 2) {
            return toRatio.MultiplyScalar(GetMagnitude(startValue, endValue, fromRatio)).Adjust(startValue).ToDouble();
        }

        public static Amount ChangeRatio(Amount startValue, Amount endValue, AmountModifier toRatio, double fromRatio = 2) {
            return toRatio.MultiplyScalar(GetMagnitude(startValue, endValue, fromRatio)).Adjust(startValue);
        }

        public static double GetMagnitude(double startValue, double endValue, double magRatio = 2) {
            return Math.Log(Math.Abs(endValue / startValue)) / Math.Log(magRatio);
        }

        public static Amount GetMagnitude(Amount startValue, Amount endValue, double magRatio = 2) {
            return (endValue / startValue).Abs().Log10() / Math.Log10(magRatio);
        }

        public static Amount DiminishingReturn(Amount value, Amount maxValue) =>
            DiminishingReturn(value, maxValue, maxValue);
        public static Amount DiminishingReturn(Amount value, Amount difficulty, Amount maxValue) {
            return (value) / (difficulty + value) * maxValue;
        }

        public static Amount RoundToZero(Amount val) {
            return Amount.Abs(val) < 0.01 ? 0 : val;
        }
        public static double RoundToZero(double val) {
            return Math.Abs(val) < 0.01 ? 0 : val;
        }

        public static double TowardsZero(double value, double subtraction) {
            return value > 0 ? Math.Max(value - subtraction, 0) : Math.Min(value + subtraction, 0);
        }

        // public static float TowardsZero(float value, float subtraction) {
        //     return value > 0 ? Mathf.Max(value - subtraction, 0) : Mathf.Min(value + subtraction, 0);
        // }

        // n = 0 returns start
        // n = 1 returns next element in sequence
        public static Amount SumArithmeticSequenceDiff(Amount start, Amount diff, Amount n) {
            return SumArithmeticSequence(start, start + (diff * n), n + 1);
        }

        public static Amount SumArithmeticSequence(Amount start, Amount end, Amount n) {
            return n / 2 * (start + end);
        }

        // a = start
        // n = 0 returns a
        public static Amount SumGeometricSequence(Amount a, Amount r, Amount n) {
            // if (r == 1) throw new ArgumentOutOfRangeException(nameof(r), "cannot be 1.");
            if (r == 1) return a * (n + 1);
            return a * (1 - Amount.Pow(r, n + 1)) / (1 - r);
        }

        public static Amount AdjustGeometricMagSequence(Amount value, Amount magnitude, Amount ratio, long softCapOrder = 0) {
            Amount softCap = Amount.Pow(magnitude, softCapOrder);
            if (value <= softCap || Approx(ratio, 1)) return value;
            
            var sequences = Amount.Log(value, magnitude) - 1 - softCapOrder;
            var actualRatio = magnitude * ratio;
            var initial = (softCap - softCap / magnitude) * actualRatio;
            return SumGeometricSequence(initial, actualRatio, sequences) + softCap;
        }

        public static Amount FindArithmeticSequenceDiffN(Amount sum, Amount start, Amount diff) {
            if (sum == start) return 1; // At start of sequence
            if ((diff > 0 && sum < start) || (diff < 0 && sum > start)) return 0; // Invalid sum
            if (diff == 0) return sum / start; // Can simply divide.
            
            // quadratic formula
            var a = (diff / 2);
            var b = (start - diff / 2);
            var c = -sum;
            var quad1 = (-b + Amount.Sqrt(Amount.Pow(b, 2) - 4 * a * c)) / (2 * a);
            var quad2 = (-b - Amount.Sqrt(Amount.Pow(b, 2) - 4 * a * c)) / (2 * a);
            if (quad1 > 0) return quad1;
            if (quad2 > 0) return quad2;
            throw new ArgumentOutOfRangeException(nameof(sum), "Invalid sum, something went wrong.");
        }

        // a = start
        // This will assume the first value in the sequence, 0 = A.
        public static Amount FindGeometricSequenceN(Amount sum, Amount a, Amount r) {
            if (r == 1) throw new ArgumentOutOfRangeException(nameof(r), "cannot be 1.");
            return Amount.Log(1 - (sum / a) * (1 - r), r) - 1;
        }

        public static int GetRandomIndexRemoved(int count, int removeIndex) {
            if (count <= 1) return 0;
            if (removeIndex < 0) return Rng.Next(0, count);
            var rawIndex = Rng.Next(0, count - 1);

            return rawIndex >= removeIndex ? rawIndex + 1 : rawIndex;
        }

        public static Amount GetEulersDiff(Amount value, Amount rate) {
            return (value * Amount.Pow(Amount.Eulers, rate)) - value;
        }

        // https://stackoverflow.com/questions/16545724/converting-base64-value-to-number
        public static long GetBase64Value(string base64String) {
            byte[] output = Convert.FromBase64String(base64String);
            Console.WriteLine(output.Length);
            int rslt = 0;
            for (int i = 0; i < output.Length; ++i) {
                rslt <<= 8;
                rslt += output[i];
            }
            
            return rslt;
        }

        /// Splits a multiplier into some stacking and some multiplier by a penalty and then calculates the new percentage.
        /// <param name="triggers">The scalar multiplier.</param>
        /// <param name="baseMultiplier">The base multiplier value. 25 = x25.</param>
        /// <param name="penalty">How much is converted to a stacking mult, and the rest is converted to diminishing. Value between 0 and 1f.</param>
        /// <returns>The calculated trigger multiplier as a Amount.</returns>
        /// /
        public static Amount DissectMultiplier(Amount triggers, Amount baseMultiplier, Amount penalty) {
            if (penalty < 0 || penalty > 1) throw new ArgumentOutOfRangeException(nameof(penalty), "penalty must be between 0 and 1.");
            if (baseMultiplier < 1) return Amount.Pow(baseMultiplier, triggers);
            Amount stackingVal = (baseMultiplier - 1) * penalty + 1;
            AmountModifier diminishMult = AmountModifier.Diminishing((baseMultiplier / stackingVal) - 1).MultiplyScalar(triggers);
            AmountModifier stackingMult = AmountModifier.StackingRaw(stackingVal).MultiplyScalar(triggers);
            return stackingMult.Adjust(diminishMult.Adjust(1));
        }
        
        /**
        * Logic
        */
        public static int GetWeightedIndex(List<float> weights) {
            if (weights.Count == 0) return -1;
            if (weights.Count == 1) return 0;
            
            var total = weights.Aggregate((sum, weight) => sum + weight);
            var chosenWeight = (float)(Rng.NextDouble() * total);
            var iteratedWeight = chosenWeight;
            for (var index = 0; index < weights.Count; index++) {
                var weight = weights[index];
                iteratedWeight -= weight;
                if (iteratedWeight <= 0) return index;
            }

            throw new ArgumentException();
        }

        /**
        * List
        */
        public static List<int> CreateIndexList(int size) {
            var indexList = new List<int>();
            for (int i = 0; i < size; i++) {
                indexList.Add(i);
            }

            return indexList;
        }
        
        public static T Sample<T>(List<T> elements) {
            return elements[Rng.Next(0, elements.Count - 1)];
        }
        
        public static IEnumerable<T> SampleSize<T>(IEnumerable<T> elements, int sampleSize) {
            return elements.OrderBy(x =>  Guid.NewGuid()).Take(sampleSize);
        }

        public static Amount CombineMod<T>(IEnumerable<T> elements, Func<T, Amount> func) {
            return elements.Aggregate(Amount.One, (total, el) => total * func(el) / 100);
        }

        public static Amount CombinePercent<T>(IEnumerable<T> elements, Func<T, Amount> func) {
            return elements.Aggregate(Amount.One, (total, el) => total * func(el));
        }

        public static T MaxBy<T, TV>(List<T> elements, Func<T, TV> func) where TV : IComparable {
            if (!elements.Any()) return default(T);

            T el = elements[0];
            TV val = func(el);
            for (int i = 1; i < elements.Count; i++) {
                T nextEl = elements[i];
                TV nextVal = func(nextEl);
                int comparison = val.CompareTo(nextVal);
                if (comparison < 0) {
                    el = nextEl;
                    val = nextVal;
                }
            }

            return el;
        }

        public static T MinBy<T, TV>(List<T> elements, Func<T, TV> func) where TV : IComparable {
            if (!elements.Any()) return default(T);

            T el = elements[0];
            TV val = func(el);
            for (int i = 1; i < elements.Count; i++) {
                T nextEl = elements[i];
                TV nextVal = func(nextEl);
                int comparison = val.CompareTo(nextVal);
                if (comparison > 0) {
                    el = nextEl;
                    val = nextVal;
                }
            }

            return el;
        }

        public static int SortCompare(params int[] sortCompares) {
            foreach (var compare in sortCompares) {
                if (compare != 0) return compare;
            }

            return 0;
        }

        public static T GetWithin<T>(List<T> elements, int index) {
            if (elements.Count <= 0) throw new Exception("GetWithin requires an list of size 1 or more.");

            var actualIndex = (index + elements.Count) % elements.Count;
            return elements[actualIndex];
        }

        public static List<T> FillListEmpty<T>(List<T> elements, int toSize) where T : class {
            if (elements.Count == toSize) {
                return new List<T>(elements);
            }
            
            if (elements.Count > toSize) {
                return elements.Take(toSize).ToList();
            }
            
            var newList = new List<T>(elements);
            
            for (int i = elements.Count; i < toSize; i++) {
                newList.Add(null);
            }

            return newList;
        }
        public static List<T> FillList<T>(List<T> elements, int toSize) where T : new() {
            if (elements.Count == toSize) {
                return new List<T>(elements);
            }
            
            if (elements.Count > toSize) {
                return elements.Take(toSize).ToList();
            }
            
            var newList = new List<T>(elements);
            
            for (int i = elements.Count; i < toSize; i++) {
                newList.Add(new T());
            }

            return newList;
        }

        // This func assumes a starting level of 0.
        public static int CalculateLevelsGained(Amount xp, Amount baseReq, AmountModifier modPerLevel, int currentLevel) {
            return modPerLevel.FindSequenceN(xp, baseReq, currentLevel).ToInt();
        }
        
        /**
        * Color
        */
        // public static Color SetAlpha(Color color, float alpha) {
        //     return new Color(color.r, color.g, color.b, alpha);
        // }

        // public static Color MultiplySaturation(Color color, float saturationMult) {
        //     Color.RGBToHSV(color, out var hue, out var sat, out var val);
        //     return Color.HSVToRGB(hue, sat * saturationMult, val);
        // }

        // public static Color MultiplyBrightness(Color color, float brightnessMult) {
        //     Color.RGBToHSV(color, out var hue, out var sat, out var val);
        //     return Color.HSVToRGB(hue, sat, val * brightnessMult);
        // }

        // public static Color WhiteTextReadable(Color color) {
        //     return Utils.MultiplyBrightness(Utils.MultiplySaturation(color, 0.8f), 0.7f);
        // }
        
        /**
        * Rotation
        */
        // public static float GetDirectionAngleDeg(Vector2 direction) {
        //     return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // }

        // public static Quaternion GetDirectionQuaternion(Vector2 direction) {
        //     return Quaternion.AngleAxis(GetDirectionAngleDeg(direction), Vector3.forward);
        // }

        // public static Vector2 GetQuaternionDirection(Quaternion quat) {
        //     return quat * Vector2.right; 
        // }

        // public static Quaternion GetAngleQuaternion(float angleRad) {
        //     return Quaternion.AngleAxis(angleRad, Vector3.forward);
        // }

        // public static float GetQuaternionAngle(Quaternion rotation) {
        //     return Quaternion.Angle(rotation, GetDirectionQuaternion(Vector2.right));
        // }


        // public static string TabulatePrint(List<string> strings, int columns) {
        //     columns = Math.Max(columns, 1);
        //     int rows = Math.CeilToInt((double)strings.Count / columns);
        //     int maxStringSize = Utils.MaxBy(strings, s => s.Length).Length;
        //     int maxIndexSize = strings.Count.ToString().Length;
        //     int columnWidth = maxStringSize + maxIndexSize + 5;
            
        //     List<string> rowStrings = new();
        //     for (int y = 0; y < rows; y++) {
        //         List<string> columnStrings = new();
        //         for (int x = 0; x < columns; x++) {
        //             int pos = y + (x * rows);
        //             if (pos >= strings.Count) continue;
        //             columnStrings.Add($"({pos.ToString().PadLeft(maxIndexSize, '0')}): {strings[pos]}".PadRight(columnWidth, ' '));
        //         }
                
        //         rowStrings.Add(string.Join(" || ", columnStrings));
        //     }
            
        //     return string.Join("\n", rowStrings);
        // }
    }
}