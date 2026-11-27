using System;
using Marple.Core;

namespace ChronoDigger.Combat {
    // This is an either-or class for inputing via the inspector.
    // Its either a reference to a parameter's value in a table
    // Or its a raw value.
    [Serializable]
    public class ParameterValue {
        public ParameterDefinition Parameter;
        public double Value;
        public ParameterMode Mode;

        public enum ParameterMode {
            Parameter,
            Value,
        }

        public Amount GetValue(ActionContext context) => GetValue(context.Parameters);
        public Amount GetValue(ParameterTable table) {
            return Mode switch {
                ParameterMode.Parameter => table.GetValue(Parameter),
                ParameterMode.Value => Value,
                _ => throw new ArgumentOutOfRangeException(nameof(Mode), $"Mode {Mode} doesn't exist."),
            };
        }
    }
}