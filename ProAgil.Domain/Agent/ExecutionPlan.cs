using System;

namespace ProAgil.Domain.Agent
{
    // Classes de suporte para execução multi-etapa
    public class ExecutionPlan
    {
        public bool IsMultiStep { get; set; }
        public string? UserMessage { get; set; }
        public List<ExecutionStep> Steps { get; set; } = new List<ExecutionStep>();
    }
}
