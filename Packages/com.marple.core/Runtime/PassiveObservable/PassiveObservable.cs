using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Marple.Core {
    public class PassiveObservable : IPassiveObservable {
        private int observedId;

        public int GetObservableId() {
            return observedId;
        }

        public void UpdateObservable() {
            observedId += 1;
        }

        public PassiveObservable() {
            observedId = 0;
        }

        /**
         * Subclass
         */
        public interface IPassiveChannel {
            public IPassiveObservable Observe(string name);
            public IEnumerable<KeyValuePair<string, PassiveObservable>> GetEntries();
        }

        public class Channel : IPassiveChannel {
            private Dictionary<string, PassiveObservable> dictionary = new Dictionary<string, PassiveObservable>();

            public Channel(params string[] names) {
                Setup(names);
            }

            public void Setup(params string[] names) => Setup(names.ToList());
            public void Setup(IEnumerable<string> names) {
                foreach (var name in names) {
                    dictionary[name] = new PassiveObservable();
                }
            }

            public Channel Conditional(params string[] names) => Conditional(names.ToList());
            public Channel Conditional(List<string> names) {
                foreach (var name in names) {
                    dictionary[name] = null;
                }

                return this;
            }

            public IPassiveObservable Observe(string name) {
                if (!dictionary.ContainsKey(name)) throw new ArgumentOutOfRangeException("Invalid PassiveObservable.Channel name: " + name);
                return dictionary[name];
            }

            public IEnumerable<IPassiveObservable> ObserveAll(params string[] names) {
                return names.Select(Observe);
            }

            public void Update(string name) {
                if (!dictionary.ContainsKey(name)) throw new ArgumentOutOfRangeException("Invalid PassiveObservable.Channel name: " + name);
                dictionary[name]?.UpdateObservable();
            }

            public void Merge(IPassiveChannel other, string prefix) {
                foreach (var keyValuePair in other.GetEntries()) {
                    dictionary[prefix + keyValuePair.Key] = keyValuePair.Value;
                }
            }

            public IEnumerable<KeyValuePair<string, PassiveObservable>> GetEntries() => dictionary.ToList();

            public ObservingCache.Channel CreateObservingChannel() => new ObservingCache.Channel(this);
        }
    }
}
