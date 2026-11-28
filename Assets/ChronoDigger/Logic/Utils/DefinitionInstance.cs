using System;

namespace ChronoDigger.Utils {
    public abstract class DefinitionInstance {

    }

    [Serializable]
    public class DefinitionInstance<TDefinition> : DefinitionInstance where TDefinition : IdScriptableObject {
        public IdObjectReference<TDefinition> DefinitionReference;
        
        public TDefinition Definition => DefinitionReference.Reference;
    }
}