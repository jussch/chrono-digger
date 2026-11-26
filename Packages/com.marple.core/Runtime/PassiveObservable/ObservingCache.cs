using System;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
    /// <summary>
    /// Remembers the ids of a set of <see cref="IPassiveObservable"/>s so you can ask
    /// "has anything changed since I last looked?" without any events.
    /// Use <see cref="IsUpdated"/>: it returns true once per change and then re-caches.
    /// </summary>
    public class ObservingCache {
        private List<int> _cacheIds;
        private List<IPassiveObservable> _passiveObservables;
        private bool _forceUpdate;
        private bool _alwaysUpdate;

        public ObservingCache(IEnumerable<IPassiveObservable> observables) {
            _passiveObservables = observables.ToList();
            _cacheIds = _passiveObservables.Select(oso => oso.GetObservableId()).ToList();
        }

        public ObservingCache(params IPassiveObservable[] observables) : this((IEnumerable<IPassiveObservable>)observables) { }

        public static ObservingCache Clean(params IPassiveObservable[] observables) {
            return new ObservingCache(observables.Where(obs => obs != null));
        }

        public static ObservingCache Clean(IEnumerable<IPassiveObservable> observables) {
            return new ObservingCache(observables.Where(obs => obs != null));
        }

        public static ObservingCache Always() => new ObservingCache().AlwaysUpdate();

        public ObservingCache CleanAdd(IPassiveObservable observable) {
            if (observable == null) return this;
            return Add(observable);
        }
        public ObservingCache CleanAdd(IEnumerable<IPassiveObservable> moreObservables) {
            return Add(moreObservables.Where(obs => obs != null));
        }

        public ObservingCache Add(IPassiveObservable observable) => Add(new[] { observable });
        public ObservingCache Add(IEnumerable<IPassiveObservable> moreObservables) {
            var enumerable = moreObservables.ToList();
            _cacheIds.AddRange(enumerable.Select(obs => obs.GetObservableId()));
            _passiveObservables.AddRange(enumerable);
            return this;
        }

        public ObservingCache Remove(IPassiveObservable observable) => Remove(new[] { observable });
        public ObservingCache Remove(IEnumerable<IPassiveObservable> removeObservables) {
            foreach (var observable in removeObservables) {
                int index = _passiveObservables.IndexOf(observable);
                if (index >= 0) {
                    _passiveObservables.RemoveAt(index);
                    _cacheIds.RemoveAt(index);
                }
            }
            return this;
        }

        /// <summary>
        /// True when nothing has changed since the last check. Prefer <see cref="IsUpdated"/>,
        /// which reads the right way round. Either call re-caches the ids.
        /// </summary>
        public virtual bool Check() {
            if (_forceUpdate) {
                _forceUpdate = _alwaysUpdate;
                return false;
            }
            return Compare(_passiveObservables);
        }

        /// <summary>True when something changed (or an update was forced) since the last check.</summary>
        public bool IsUpdated() {
            return !Check();
        }

        public void RefreshCache() {
            OverwriteCacheIds(_passiveObservables);
            _forceUpdate = false;
        }

        private void OverwriteCacheIds(List<IPassiveObservable> observables) {
            _cacheIds.Clear();
            for (int i = 0; i < observables.Count; i++) {
                _cacheIds.Add(observables[i].GetObservableId());
            }
        }

        public ObservingCache AlwaysUpdate() {
            _alwaysUpdate = true;
            _forceUpdate = true;
            return this;
        }

        public ObservingCache ForceUpdate() => EnsureUpdate();
        public ObservingCache EnsureUpdate() {
            _forceUpdate = true;
            return this;
        }

        public bool Compare(IPassiveObservable observable) {
            if (_cacheIds.Count != 1) {
                throw new InvalidOperationException("Invalid comparison, trying to compare " + _cacheIds.Count + " items to 1");
            }

            int id = observable.GetObservableId();
            bool comparison = id == _cacheIds[0];
            if (!comparison) _cacheIds[0] = id;
            return comparison;
        }

        public bool Compare(List<IPassiveObservable> observables) => FindFirstChanged(observables) < 0;

        /// <summary>Same as <see cref="Check"/> but ignores forced updates; set a breakpoint or log in FindFirstChanged.</summary>
        public bool DebugCheck() => Compare(_passiveObservables);

        public bool DebugIsUpdated() => !DebugCheck();

        // Returns the index of the first observable whose id changed (re-caching all ids), or -1.
        private int FindFirstChanged(List<IPassiveObservable> observables) {
            if (_cacheIds.Count != observables.Count) {
                throw new InvalidOperationException("Invalid comparison, trying to compare " + _cacheIds.Count + " items to " + observables.Count);
            }

            for (int i = 0; i < observables.Count; i++) {
                if (observables[i].GetObservableId() != _cacheIds[i]) {
                    // Debug.Log("Element: " + i + " failed.");
                    OverwriteCacheIds(observables);
                    return i;
                }
            }
            return -1;
        }

        /**
         * Subclasses
         */
        public class Channel {
            private Dictionary<string, ObservingCache> _dictionary;

            public Channel(PassiveObservable.IPassiveChannel channel) {
                _dictionary = new Dictionary<string, ObservingCache>();
                foreach (var keyValuePair in channel.GetEntries()) {
                    _dictionary.Add(keyValuePair.Key, ObservingCache.Clean(keyValuePair.Value));
                }
            }

            public bool IsUpdated(string name) => _dictionary[name].IsUpdated();
            public void RefreshCache(string name) => _dictionary[name].RefreshCache();
            public void EnsureUpdate(string name) => _dictionary[name].EnsureUpdate();
            public void Add(string name, IPassiveObservable element) => _dictionary[name].Add(element);
            public void Add(string name, IEnumerable<IPassiveObservable> moreObservables) => _dictionary[name].Add(moreObservables);
            public void Remove(string name, IPassiveObservable element) => _dictionary[name].Remove(element);
            public void Remove(string name, IEnumerable<IPassiveObservable> moreObservables) => _dictionary[name].Remove(moreObservables);

            public Channel RefreshAllCachesExpect(params string[] exceptNames) {
                foreach (var key in _dictionary.Keys) {
                    if (exceptNames.Contains(key)) continue;
                    _dictionary[key].RefreshCache();
                }

                return this;
            }

            public Channel RefreshAllCaches() {
                foreach (var cache in _dictionary.Values) {
                    cache.RefreshCache();
                }

                return this;
            }

            public Channel EnsureUpdateAll() {
                foreach (var cache in _dictionary.Values) {
                    cache.EnsureUpdate();
                }

                return this;
            }
        }

        // Unity-only: checks at most once per frame. Restore when this runs inside Unity
        // (needs `using UnityEngine;` for Time.frameCount).
        // public class FrameLocked : ObservingCache {
        //     private int frame = -1;
        //     private bool updated;
        //
        //     public FrameLocked(IEnumerable<IPassiveObservable> observables) : base(observables) { }
        //
        //     public FrameLocked(params IPassiveObservable[] observables) : base(observables) { }
        //
        //     public override bool Check() {
        //         if (frame == Time.frameCount) return updated;
        //         frame = Time.frameCount;
        //         updated = base.Check();
        //         return updated;
        //     }
        // }
    }
}
