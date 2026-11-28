using System;

namespace ChronoDigger.Utils {
    // For Serializing IdObjectReferences
    public abstract class IdObjectReference {
        public string Id;

        public IdScriptableObject GetIdScriptableObject() {
            if (string.IsNullOrEmpty(Id)) return null;
            return IdScriptableObject.GetFromRegistry(Id);
        }
    }

    [Serializable]
    public class IdObjectReference<T> : IdObjectReference where T : IdScriptableObject  {
        public T Reference => GetReference();

        public T GetReference() {
            var idObj = GetIdScriptableObject();
            return idObj != null ? (T)idObj : null;
        }
    }
}