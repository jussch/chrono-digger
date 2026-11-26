using NUnit.Framework;
using Marple.Core;

namespace Marple.Core.Tests {
    public class AmountModifierRecordTests {
        private class TestDescribable : IDescribable {
            public string DisplayName => "Upgrade";
            public string Description => "A test upgrade";
        }

        private readonly object _source = new object();

        [Test]
        public void Add_BareModifier_StoresEntryWithNoDisplayInfo() {
            var record = new AmountModifierRecord();
            record.Add(_source, AmountModifier.Raw(5));

            var entry = record.Modifiers[new AmountModifierKey(_source)];
            Assert.AreEqual(AmountModifier.Raw(5), entry.Modifier);
            Assert.IsNull(entry.Describable);
            Assert.IsFalse(entry.State.HasColor());
            Assert.AreEqual((Amount)15, record.Adjust(10));
        }

        [Test]
        public void Add_FluentEntry_StoresDescribableAndState() {
            var record = new AmountModifierRecord();
            var describable = new TestDescribable();
            record.Add(_source, 2, AmountModifier.Raw(5).With(describable).With(TextState.Insufficient));

            var entry = record.Modifiers[new AmountModifierKey(_source, 2)];
            Assert.AreSame(describable, entry.Describable);
            Assert.AreEqual(TextState.TextStateType.Insufficient, entry.State.Type);
        }

        [Test]
        public void With_StateFirst_ThenDescribable_KeepsBoth() {
            var describable = new TestDescribable();
            var entry = AmountModifier.Raw(1).With(TextState.Insufficient).With(describable);

            Assert.AreSame(describable, entry.Describable);
            Assert.AreEqual(TextState.TextStateType.Insufficient, entry.State.Type);
        }

        [Test]
        public void WithModifier_PreservesDisplayInfo() {
            var describable = new TestDescribable();
            var entry = AmountModifier.Raw(1).With(describable).With(TextState.Insufficient)
                .WithModifier(AmountModifier.Raw(2));

            Assert.AreEqual(AmountModifier.Raw(2), entry.Modifier);
            Assert.AreSame(describable, entry.Describable);
            Assert.AreEqual(TextState.TextStateType.Insufficient, entry.State.Type);
        }

        [Test]
        public void Add_EmptyModifier_RemovesKey() {
            var record = new AmountModifierRecord();
            record.Add(_source, AmountModifier.Raw(5));
            record.Add(_source, AmountModifier.Raw(0).With(TextState.Insufficient));

            Assert.IsFalse(record.Has(_source));
        }

        [Test]
        public void Toggle_AddsAndRemovesEntry() {
            var record = new AmountModifierRecord();
            var key = new AmountModifierKey(_source);

            record.Toggle(key, AmountModifier.Raw(3).With(TextState.Insufficient), true);
            Assert.AreEqual(TextState.TextStateType.Insufficient, record.Modifiers[key].State.Type);

            record.Toggle(key, AmountModifier.Raw(3), false);
            Assert.IsFalse(record.Has(key));
        }

        [Test]
        public void MergingRecord_PropagatesDisplayInfoToMergedRecords() {
            var merging = new MergingModifierRecord();
            var target = new AmountModifierRecord();
            merging.AddRecord(target);

            var describable = new TestDescribable();
            merging.Add(_source, AmountModifier.Raw(5).With(describable).With(TextState.Insufficient));

            var entry = target.Modifiers[new AmountModifierKey(_source)];
            Assert.AreSame(describable, entry.Describable);
            Assert.AreEqual(TextState.TextStateType.Insufficient, entry.State.Type);
        }

        [Test]
        public void ModifierList_GetRecord_AppliesDisplayInfoToEveryEntry() {
            var list = new AmountModifierList(AmountModifier.Raw(1), AmountModifier.Raw(2));
            var describable = new TestDescribable();
            var record = list.GetRecord(_source, describable, TextState.Insufficient);

            foreach (var entry in record.Modifiers.Values) {
                Assert.AreSame(describable, entry.Describable);
                Assert.AreEqual(TextState.TextStateType.Insufficient, entry.State.Type);
            }
            Assert.AreEqual(2, record.Modifiers.Count);
        }
    }
}
