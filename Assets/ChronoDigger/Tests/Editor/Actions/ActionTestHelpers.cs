using ChronoDigger.Combat;

namespace ChronoDigger.Tests {
    internal static class ActionTestHelpers {
        public static readonly GridPos PlayerPos = new GridPos(2, 0, -1);

        public static Actor ActorAt(int row, int column = 2) {
            return new Actor { Position = new GridPos(column, 0, row) };
        }

        public static Actor Player() => new Actor { Position = PlayerPos };
    }
}
