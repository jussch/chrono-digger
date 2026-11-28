using System.Collections.Generic;
using Marple.Core;

namespace ChronoDigger.Utils {
    public abstract class IdScriptableObject /* : ScriptableObject */ : IHasId {
        public string Id => GetId();
        private string _id;

        public void ApplyId(string id) {
            _id = id;
            // EditorUtilities.SetDirty(this);
        }

        private static Dictionary<string, IdScriptableObject> _idRegistry = new Dictionary<string, IdScriptableObject>();
        public static IdScriptableObject GetFromRegistry(string id) {
            return _idRegistry.TryGetValue(id, out IdScriptableObject idObject) ? idObject : null;
        }

        // TODO: The below code is only for when we aren't serializing IdScriptableObjects.
        // When that happens we'll need a different solution.
        private string GetId() {
            if (string.IsNullOrEmpty(_id)) {
                ApplyId(GenerateId());
                _idRegistry[_id] = this;
            }
            return _id;
        }

        private static int _generatedIds = 0;
        private static string GenerateId() {
            _generatedIds++;
            return $"id_{_generatedIds}";
        }
    }
}