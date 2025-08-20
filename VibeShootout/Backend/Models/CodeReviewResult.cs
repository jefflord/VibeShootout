using System;

namespace VibeShootout.Backend.Models
{
    public class CodeReviewResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string FilePath { get; set; } = "";
        public string Diff { get; set; } = "";
        public string Review { get; set; } = "";
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}