using System;
using VibeShootout.Backend.Services;

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
        
        // Timing properties
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public double DurationMs => Duration.TotalMilliseconds;
        
        // Checksum property for duplicate detection
        public string DiffChecksum { get; set; } = "";
        
        // Ollama performance metrics
        public OllamaMetrics? OllamaMetrics { get; set; }
    }
}