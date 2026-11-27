using ChronoDigger.Combat;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class AmountTableTests {
        private readonly object _buff = new object();

        [Test]
        public void SnapShot_KeepsRuntimeType() {
            var block = new StatTable(TestStats.Profile());

            Assert.IsInstanceOf<StatTable>(block.SnapShot());
        }

        [Test]
        public void SnapShot_ModifiedEntry_BaseIsCalculatedValueWithNoModifiers() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatTable(TestStats.Profile());
            block.Get(defence).Add(_buff, AmountModifier.Raw(3));

            var snapshot = block.SnapShot();

            Assert.AreEqual(10d, snapshot.Get(defence).BaseValue);
            Assert.IsTrue(snapshot.Get(defence).IsEmpty());
        }

        [Test]
        public void SnapShot_OriginalChangedAfterwards_SnapshotUnchanged() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatTable(TestStats.Profile((defence, 7)));
            var snapshot = block.SnapShot();

            block.Get(defence).Add(_buff, AmountModifier.Raw(3));

            Assert.AreEqual((Amount)7, snapshot.GetValue(defence));
        }

        [Test]
        public void SnapShot_SnapshotChanged_OriginalUnchanged() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatTable(TestStats.Profile((defence, 7)));
            var snapshot = block.SnapShot();

            snapshot.Get(defence).Add(_buff, AmountModifier.Raw(3));

            Assert.AreEqual((Amount)7, block.GetValue(defence));
        }
    }
}
