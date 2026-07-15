using System.Diagnostics;

namespace SqlAgent.Agent
{
    public class AgentContext
    {
        public string Question { get; set; }

        public string Sql { get; set; }

        public IEnumerable<dynamic>? Data { get; set; }

        public string? Explanation { get; set; }

        public bool Success { get; set; }

        public string? Error { get; set; }

        public int Attempt { get; set; }

        public int MaxAttempts { get; set; } = 3;

        public Stopwatch TotalTimer { get; } = Stopwatch.StartNew();

        public Stopwatch SqlTimer { get; } = new();

        public Stopwatch DbTimer { get; } = new();

        public Stopwatch ExplainTimer { get; } = new();

        public List<string> Steps { get; } = new();

        public List<string> Errors { get; } = new();

        public bool SqlCorrected { get; set; }

        public bool EmptyResultRetry { get; set; }

        public bool SchemaValidated { get; set; }
    }
}
