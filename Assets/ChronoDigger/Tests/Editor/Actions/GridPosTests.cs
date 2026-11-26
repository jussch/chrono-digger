using ChronoDigger.Combat;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class GridPosTests {
        [Test]
        public void Equals_SameCoordinates_True() {
            Assert.AreEqual(new GridPos(1, 2, 3), new GridPos(1, 2, 3));
            Assert.IsTrue(new GridPos(1, 2, 3) == new GridPos(1, 2, 3));
        }

        [Test]
        public void Equals_DifferentCoordinates_False() {
            Assert.AreNotEqual(new GridPos(1, 2, 3), new GridPos(1, 2, 4));
            Assert.IsTrue(new GridPos(1, 2, 3) != new GridPos(3, 2, 1));
        }

        [Test]
        public void RowDistance_IgnoresColumnAndHeight() {
            Assert.AreEqual(2, GridPos.RowDistance(new GridPos(0, 0, -1), new GridPos(5, 3, 1)));
        }
    }
}
