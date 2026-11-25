using NUnit.Framework;
using Marple.Core;

namespace Marple.Core.Tests
{
    public class AmountModifierRecordTests
    {
        private class TestDescribable : IDescribable
        {
            public string DisplayName => "Upgrade";
            public string Description => "A test upgrade";
        }

        private readonly object source = new object();

        [Test]
        public void Add_BareModifier_StoresEntryWithNoDisplayInfo()
        {
            var record = new AmountModifierRecord();
            record.Add(source, AmountModifier.Raw(5));

            var entry = record.modifiers[new AmountModifierKey(source)];
            Assert.AreEqual(AmountModifier.Raw(5), entry.Modifier);
            Assert.IsNull(entry.Describable);
            Assert.IsFalse(entry.State.HasColor());
            Assert.AreEqual((Amount)15, record.Adjust(10));
        }

        [Test]
        public void Add_FluentEntry_StoresDescribableAndState()
        {
            var record = new AmountModifierRecord();
            var describable = new TestDescribable();
            record.Add(source, 2, AmountModifier.Raw(5).With(describable).With(TextState.insufficient));

            var entry = record.modifiers[new AmountModifierKey(source, 2)];
            Assert.AreSame(describable, entry.Describable);
            Assert.AreEqual(TextState.TextStateType.Insufficient, entry.State.type);
        }

        [Test]
        public void With_StateFirst_ThenDescribable_KeepsBoth()
        {
            var describable = new TestDescribable();
            var entry = AmountModifier.Raw(1).With(TextState.insufficient).With(describable);

            Assert.AreSame(describable, entry.Describable);
            Assert.AreEqual(TextState.TextStateType.Insufficient, entry.State.type);
        }

        [Test]
        public void WithModifier_PreservesDisplayInfo()
        {
            var describable = new TestDescribable();
            var entry = AmountModifier.Raw(1).With(describable).With(TextState.insufficient)
                .WithModifier(AmountModifier.Raw(2));

            Assert.AreEqual(AmountModifier.Raw(2), entry.Modifier);
            Assert.AreSame(describable, entry.Describable);
            Assert.AreEqual(TextState.TextStateType.Insufficient, entry.State.type);
        }

        [Test]
        public void Add_EmptyModifier_RemovesKey()
        {
            var record = new AmountModifierRecord();
            record.Add(source, AmountModifier.Raw(5));
            record.Add(source, AmountModifier.Raw(0).With(TextState.insufficient));

            Assert.IsFalse(record.Has(source));
        }

        [Test]
        public void Toggle_AddsAndRemovesEntry()
        {
            var record = new AmountModifierRecord();
            var key = new AmountModifierKey(source);

            record.Toggle(key, AmountModifier.Raw(3).With(TextState.insufficient), true);
            Assert.AreEqual(TextState.TextStateType.Insufficient, record.modifiers[key].State.type);

            record.Toggle(key, AmountModifier.Raw(3), false);
            Assert.IsFalse(record.Has(key));
        }

        [Test]
        public void MergingRecord_PropagatesDisplayInfoToMergedRecords()
        {
            var merging = new MergingModifierRecord();
            var target = new AmountModifierRecord();
            merging.AddRecord(target);

            var describable = new TestDescribable();
            merging.Add(source, AmountModifier.Raw(5).With(describable).With(TextState.insufficient));

            var entry = target.modifiers[new AmountModifierKey(source)];
            Assert.AreSame(describable, entry.Describable);
            Assert.AreEqual(TextState.TextStateType.Insufficient, entry.State.type);
        }

        [Test]
        public void ModifierList_GetRecord_AppliesDisplayInfoToEveryEntry()
        {
            var list = new AmountModifierList(AmountModifier.Raw(1), AmountModifier.Raw(2));
            var describable = new TestDescribable();
            var record = list.GetRecord(source, describable, TextState.insufficient);

            foreach (var entry in record.modifiers.Values)
            {
                Assert.AreSame(describable, entry.Describable);
                Assert.AreEqual(TextState.TextStateType.Insufficient, entry.State.type);
            }
            Assert.AreEqual(2, record.modifiers.Count);
        }
    }
}
