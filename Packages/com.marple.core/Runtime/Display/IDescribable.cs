using System;

namespace Marple.Core {
    public interface IDescribable {
        public string DisplayName { get; }
        public string Description { get; }
        // public Sprite Icon { get; }
    }
}