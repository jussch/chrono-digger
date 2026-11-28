using ChronoDigger.Combat;
using ChronoDigger.Utils;
using Marple.Core;
using NUnit.Framework;

namespace ChronoDigger.Tests {
    public class AmountTableTests {
        private readonly object _buff = new object();

        [Test]
        public void Snapshot_KeepsRuntimeType() {
            var block = new StatTable(TestStats.Profile());

            Assert.IsInstanceOf<StatTable>(block.Snapshot());
        }

        [Test]
        public void Snapshot_ModifiedEntry_BaseIsCalculatedValueWithNoModifiers() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatTable(TestStats.Profile());
            block.Get(defence).Add(_buff, AmountModifier.Raw(3));

            var snapshot = block.Snapshot();

            Assert.AreEqual(10d, snapshot.Get(defence).BaseValue);
            Assert.IsTrue(snapshot.Get(defence).IsEmpty());
        }

        [Test]
        public void Snapshot_OriginalChangedAfterwards_SnapshotUnchanged() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatTable(TestStats.Profile((defence, 7)));
            var snapshot = block.Snapshot();

            block.Get(defence).Add(_buff, AmountModifier.Raw(3));

            Assert.AreEqual((Amount)7, snapshot.GetValue(defence));
        }

        [Test]
        public void Snapshot_SnapshotChanged_OriginalUnchanged() {
            var defence = TestStats.Create("Defence", 7);
            var block = new StatTable(TestStats.Profile((defence, 7)));
            var snapshot = block.Snapshot();

            snapshot.Get(defence).Add(_buff, AmountModifier.Raw(3));

            Assert.AreEqual((Amount)7, block.GetValue(defence));
        }

        [Test]
        public void Merge_NoConflict_AddsEntryWithModifiers() {
            var attack = TestStats.Create("Attack");
            var target = new StatTable(TestStats.Profile());
            var source = new StatTable(TestStats.Profile((attack, 5)));
            source.Get(attack).Add(_buff, AmountModifier.Raw(3));

            target.Merge(source, AmountTableConflicts.Throw);

            Assert.AreEqual((Amount)8, target.GetValue(attack));
            Assert.IsTrue(target.Get(attack).Has(_buff));
        }

        [Test]
        public void Merge_SourceChangedAfterwards_TargetUnchanged() {
            var attack = TestStats.Create("Attack");
            var target = new StatTable(TestStats.Profile());
            var source = new StatTable(TestStats.Profile((attack, 5)));

            target.Merge(source, AmountTableConflicts.Throw);
            source.Get(attack).Add(_buff, AmountModifier.Raw(3));

            Assert.AreEqual((Amount)5, target.GetValue(attack));
        }

        [Test]
        public void Merge_ConflictWithThrow_Throws() {
            var attack = TestStats.Create("Attack");
            var target = new StatTable(TestStats.Profile((attack, 1)));
            var source = new StatTable(TestStats.Profile((attack, 5)));

            Assert.Throws<System.InvalidOperationException>(() => target.Merge(source, AmountTableConflicts.Throw));
        }

        [Test]
        public void Merge_ConflictWithKeepExisting_KeepsExisting() {
            var attack = TestStats.Create("Attack");
            var target = new StatTable(TestStats.Profile((attack, 1)));
            var source = new StatTable(TestStats.Profile((attack, 5)));

            target.Merge(source, AmountTableConflicts.KeepExisting);

            Assert.AreEqual((Amount)1, target.GetValue(attack));
        }

        [Test]
        public void Merge_ConflictWithKeepIncoming_KeepsIncoming() {
            var attack = TestStats.Create("Attack");
            var target = new StatTable(TestStats.Profile((attack, 1)));
            var source = new StatTable(TestStats.Profile((attack, 5)));

            target.Merge(source, AmountTableConflicts.KeepIncoming);

            Assert.AreEqual((Amount)5, target.GetValue(attack));
        }

        [Test]
        public void Merge_ConflictWithSum_AddsBasesAndModifiers() {
            var attack = TestStats.Create("Attack");
            var target = new StatTable(TestStats.Profile((attack, 1)));
            var source = new StatTable(TestStats.Profile((attack, 5)));
            source.Get(attack).Add(_buff, AmountModifier.Raw(3));

            target.Merge(source, AmountTableConflicts.Sum);

            Assert.AreEqual((Amount)9, target.GetValue(attack));
        }

        [Test]
        public void Merge_CustomResolver_ReceivesDefinition() {
            var attack = TestStats.Create("Attack");
            var target = new StatTable(TestStats.Profile((attack, 1)));
            var source = new StatTable(TestStats.Profile((attack, 5)));
            StatDefinition seen = null;

            target.Merge(source, (definition, existing, incoming) => {
                seen = definition;
                return existing;
            });

            Assert.AreSame(attack, seen);
        }
    }
}
