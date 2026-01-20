using System;

namespace ProAgil.Domain.Agent
{
    // Support classes for multi-step execution
    public class ExecutionPlan
    {
        public bool IsMultiStep { get; set; }
        public string? UserMessage { get; set; }
        public List<ExecutionStep> Steps { get; set; } = new List<ExecutionStep>();
    }
}
