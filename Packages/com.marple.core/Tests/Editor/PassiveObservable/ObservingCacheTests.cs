using NUnit.Framework;
using Marple.Core;

namespace Marple.Core.Tests {
    public class ObservingCacheTests {
        [Test]
        public void IsUpdated_FalseWhenNothingChanged() {
            var observable = new PassiveObservable();
            var cache = new ObservingCache(observable);

            Assert.IsFalse(cache.IsUpdated());
        }

        [Test]
        public void IsUpdated_TrueOnceAfterChange() {
            var observable = new PassiveObservable();
            var cache = new ObservingCache(observable);

            observable.UpdateObservable();

            Assert.IsTrue(cache.IsUpdated());
            Assert.IsFalse(cache.IsUpdated());
        }

        [Test]
        public void IsUpdated_DetectsChangeInAnyObservable() {
            var first = new PassiveObservable();
            var second = new PassiveObservable();
            var cache = new ObservingCache(first, second);

            second.UpdateObservable();

            Assert.IsTrue(cache.IsUpdated());
        }

        [Test]
        public void RefreshCache_SwallowsPendingChange() {
            var observable = new PassiveObservable();
            var cache = new ObservingCache(observable);

            observable.UpdateObservable();
            cache.RefreshCache();

            Assert.IsFalse(cache.IsUpdated());
        }

        [Test]
        public void EnsureUpdate_ForcesOneUpdate() {
            var cache = new ObservingCache(new PassiveObservable()).EnsureUpdate();

            Assert.IsTrue(cache.IsUpdated());
            Assert.IsFalse(cache.IsUpdated());
        }

        [Test]
        public void Always_UpdatesEveryCheck() {
            var cache = ObservingCache.Always();

            Assert.IsTrue(cache.IsUpdated());
            Assert.IsTrue(cache.IsUpdated());
        }

        [Test]
        public void ModifiableAmount_IdChangesWhenModifierAdded() {
            var amount = new ModifiableAmount(10);
            var cache = new ObservingCache(amount);

            amount.Add(new object(), AmountModifier.Raw(5));

            Assert.IsTrue(cache.IsUpdated());
        }
    }
}
