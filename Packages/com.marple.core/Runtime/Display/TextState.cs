using System;

namespace Marple.Core {
    /// <summary>
    /// How a piece of displayed text should be styled. <c>default(TextState)</c> is
    /// <see cref="TextStateType.None"/>, meaning "no special styling".
    /// </summary>
    [Serializable]
    public struct TextState {
        public TextStateType Type;

        public enum TextStateType {
            None,
            Insufficient,
        }

        public TextState(TextStateType typeN) {
            Type = typeN;
        }

        // public Color GetColor() {
        //     switch (type) {
        //         case TextStateType.None: return Color.white;
        //         case TextStateType.Insufficient: return GlobalVariables.GetInsufficientColor();
        //         default: throw new System.NotImplementedException();
        //     }
        // }

        public bool HasColor() => Type != TextStateType.None;

        // public string Apply(string text) {
        //     if (!HasColor()) return text;

        //     return Utils.Colorize(text, GetColor());
        // }

        public override string ToString() => Type.ToString();

        public static readonly TextState None = new TextState(TextStateType.None);
        public static readonly TextState Insufficient = new TextState(TextStateType.Insufficient);

        public static TextState Cond(TextStateType state, bool condition) => condition ? new TextState(state) : None;
        public static TextState InsufficientCond(bool cond) => Cond(TextStateType.Insufficient, cond);
    }
}
