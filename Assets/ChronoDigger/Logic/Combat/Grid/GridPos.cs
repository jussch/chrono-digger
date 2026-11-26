using System;

namespace ChronoDigger.Combat {
    public readonly struct GridPos : IEquatable<GridPos> {
        public readonly int Column; // left-right
        public readonly int Height; // up-down; flyers sit above 0
        public readonly int Row;    // depth into the room; 0 is the row nearest the player, who stands at -1

        public GridPos(int column, int height, int row) {
            Column = column;
            Height = height;
            Row = row;
        }

        public static int RowDistance(GridPos a, GridPos b) => Math.Abs(a.Row - b.Row);

        public bool Equals(GridPos other) => Column == other.Column && Height == other.Height && Row == other.Row;
        public override bool Equals(object obj) => obj is GridPos other && Equals(other);
        public override int GetHashCode() => (Column * 397 ^ Height) * 397 ^ Row;
        public static bool operator ==(GridPos a, GridPos b) => a.Equals(b);
        public static bool operator !=(GridPos a, GridPos b) => !a.Equals(b);

        public override string ToString() => $"(Column {Column}, Height {Height}, Row {Row})";
    }
}
