namespace SqlAgent.DTOs
{
    public class QueryResponse
    {
        public bool Success { get; set; }

        public string Question { get; set; } = string.Empty;

        public string SqlQuery { get; set; } = string.Empty;

        public int ResultCount { get; set; }

        public object? Data { get; set; }

        public string? Explanation { get; set; }

        public string? Error { get; set; }

        public long SqlGenerationTimeMs { get; set; }

        public long DatabaseExecutionTimeMs { get; set; }

        public long ExplanationTimeMs { get; set; }

        public long TotalExecutionTimeMs { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public Guid RequestId { get; set; } = Guid.NewGuid();

    }
}
