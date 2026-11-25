using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ObservingCache {
    private List<int> cacheIds;
    private List<IPassiveObservable> passiveObservables;
    private bool forceUpdate;
    private bool alwaysUpdate;

    public ObservingCache(IEnumerable<IPassiveObservable> observables) {
        var enumerable = observables.ToList();
        cacheIds = enumerable.Select(oso => oso.GetObservableId()).ToList();
        passiveObservables = enumerable.ToList();
    }

    public ObservingCache(params IPassiveObservable[] observables) {
        cacheIds = observables.Select(oso => oso.GetObservableId()).ToList();
        passiveObservables = observables.ToList();
    }

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
        cacheIds.AddRange(enumerable.Select(obs => obs.GetObservableId()));
        passiveObservables.AddRange(enumerable);
        return this;
    }

    public ObservingCache Remove(IPassiveObservable observable) => Remove(new[] { observable });
    public ObservingCache Remove(IEnumerable<IPassiveObservable> removeObservables) {
        var enumerable = removeObservables.ToList();
        for (int i = 0; i < enumerable.Count; i++) {
            int index = passiveObservables.IndexOf(enumerable[i]);
            if (index >= 0) {
                passiveObservables.RemoveAt(index);
                cacheIds.RemoveAt(index);
            }
        }
        return this;
    }

    public virtual bool Check() {
        if (forceUpdate) {
            forceUpdate = alwaysUpdate;
            return false;
        }
        return Compare(passiveObservables);
    }

    public bool IsUpdated() {
        return !Check();
    }

    public void RefreshCache() {
        OverwriteCacheIds(passiveObservables);
        forceUpdate = false;
    }

    private void OverwriteCacheIds(List<IPassiveObservable> observables) {
        cacheIds.Clear();
        for (int i = 0; i < observables.Count; i++) {
            cacheIds.Add(observables[i].GetObservableId());
        }
    }

    public ObservingCache AlwaysUpdate() {
        alwaysUpdate = true;
        forceUpdate = true;
        return this;
    }

    public ObservingCache ForceUpdate()  => EnsureUpdate();
    public ObservingCache EnsureUpdate() {
        forceUpdate = true;
        return this;
    }

    public bool Compare(IPassiveObservable observable) {
        if (cacheIds.Count != 1) {
            throw new AggregateException("Invalid comparison, trying to compare " + cacheIds.Count + " items to " + 1);
        }
        
        bool comparison = observable.GetObservableId() == cacheIds[0];
        if (!comparison) cacheIds[0] = observable.GetObservableId();
        return comparison;
    }

    public bool Compare(List<IPassiveObservable> observables) {
        if (cacheIds.Count != observables.Count) {
            throw new AggregateException("Invalid comparison, trying to compare " + cacheIds.Count + " items to " + observables.Count);
        }
        
        bool comparison = observables.Count == cacheIds.Count;

        if (comparison) {
            for (int i = 0; i < observables.Count; i++) {
                if (observables[i].GetObservableId() != cacheIds[i]) {
                    comparison = false;
                    break;
                }
            }
        }
        
        if (!comparison) OverwriteCacheIds(observables);
        return comparison;
    }

    public bool DebugCheck() {
        return DebugCompare(passiveObservables);
    }

    public bool DebugIsUpdated() {
        return !DebugCheck();
    }
    
    public bool DebugCompare(List<IPassiveObservable> observables) {
        if (cacheIds.Count != observables.Count) {
            throw new AggregateException("Invalid comparison, trying to compare " + cacheIds.Count + " items to " + observables.Count);
        }
        
        bool comparison = observables.Count == cacheIds.Count;

        if (comparison) {
            for (int i = 0; i < observables.Count; i++) {
                if (observables[i].GetObservableId() != cacheIds[i]) {
                    comparison = false;
                    // Debug.Log("Element: " + i + " failed.");
                    break;
                }
            }
        }
        
        if (!comparison) OverwriteCacheIds(observables);
        return comparison;
    }

    /**
     * Subclasses
     */
    public class Channel {
        private Dictionary<string, ObservingCache> dictionary;

        public Channel(PassiveObservable.IPassiveChannel channel) {
            dictionary = new Dictionary<string, ObservingCache>();
            foreach (var keyValuePair in channel.GetEntries()) {
                dictionary.Add(keyValuePair.Key, ObservingCache.Clean(keyValuePair.Value));
            }
        }

        public bool IsUpdated(string name) => dictionary[name].IsUpdated();
        public void RefreshCache(string name) => dictionary[name].RefreshCache();
        public void EnsureUpdate(string name) => dictionary[name].EnsureUpdate();
        public void Add(string name, IPassiveObservable element) => dictionary[name].Add(element);
        public void Add(string name, IEnumerable<IPassiveObservable> moreObservables) => dictionary[name].Add(moreObservables);
        public void Remove(string name, IPassiveObservable element) => dictionary[name].Remove(element);
        public void Remove(string name, IEnumerable<IPassiveObservable> moreObservables) => dictionary[name].Remove(moreObservables);

        public Channel RefreshAllCachesExpect(params string[] exceptNames) {
            foreach (var key in dictionary.Keys) {
                if (exceptNames.Contains(key)) continue;
                dictionary[key].RefreshCache();
            }
            
            return this;
        }
        
        public Channel RefreshAllCaches() {
            foreach (var cache in dictionary.Values) {
                cache.RefreshCache();
            }

            return this;
        }

        public Channel EnsureUpdateAll() {
            foreach (var cache in dictionary.Values) {
                cache.EnsureUpdate();
            }

            return this;
        }
    }

    public class FrameLocked : ObservingCache {
        private int frame = -1;
        private bool updated;

        public FrameLocked(IEnumerable<IPassiveObservable> observables) : base(observables) {
            
        }

        public FrameLocked(params IPassiveObservable[] observables) : base(observables) {
            
        }

        public override bool Check() {
            if (frame == Time.frameCount) return updated;
            frame = Time.frameCount;
            updated = base.Check();
            return updated;
        }
    }
}