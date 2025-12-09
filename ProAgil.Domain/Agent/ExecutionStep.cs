using System;

namespace ProAgil.Domain.Agent
{
    public class ExecutionStep
    {
        public int Order { get; set; }
        public string? Description { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? ParameterSource { get; set; } // "user_input", "previous_step", "computed"
        public string? Transformation { get; set; } // Tipo de transformação se Action = "TRANSFORM_DATA"
        public Dictionary<string, string> ParameterMappings { get; set; } =
            new Dictionary<string, string>();
    }
}
