using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VibeShootout.Backend.Services
{
    public class OllamaResponse
    {
        public string Response { get; set; } = "";
        public OllamaMetrics? Metrics { get; set; }
    }

    public class OllamaMetrics
    {
        [JsonPropertyName("totalDuration")]
        public long TotalDuration { get; set; }
        
        [JsonPropertyName("loadDuration")]
        public long LoadDuration { get; set; }
        
        [JsonPropertyName("promptEvalCount")]
        public int PromptEvalCount { get; set; }
        
        [JsonPropertyName("promptEvalDuration")]
        public long PromptEvalDuration { get; set; }
        
        [JsonPropertyName("evalCount")]
        public int EvalCount { get; set; }
        
        [JsonPropertyName("evalDuration")]
        public long EvalDuration { get; set; }
        
        // Calculated properties with proper JSON naming
        [JsonPropertyName("totalDurationSeconds")]
        public double TotalDurationSeconds => TotalDuration / 1_000_000_000.0;
        
        [JsonPropertyName("loadDurationSeconds")]
        public double LoadDurationSeconds => LoadDuration / 1_000_000_000.0;
        
        [JsonPropertyName("promptEvalDurationSeconds")]
        public double PromptEvalDurationSeconds => PromptEvalDuration / 1_000_000_000.0;
        
        [JsonPropertyName("evalDurationSeconds")]
        public double EvalDurationSeconds => EvalDuration / 1_000_000_000.0;
        
        [JsonPropertyName("promptTokensPerSecond")]
        public double PromptTokensPerSecond => PromptEvalDurationSeconds > 0 ? PromptEvalCount / PromptEvalDurationSeconds : 0;
        
        [JsonPropertyName("outputTokensPerSecond")]
        public double OutputTokensPerSecond => EvalDurationSeconds > 0 ? EvalCount / EvalDurationSeconds : 0;
    }

    public class OllamaService
    {
        private readonly HttpClient _httpClient;

        public OllamaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OllamaResponse> GetCodeReviewWithMetricsAsync(string ollamaUrl, string prompt, string diff)
        {
            try
            {
                var fullPrompt = $"{prompt}\n\n{diff}";
                
                var requestBody = new
                {
                    model = "gpt-oss:20b",
                    prompt = fullPrompt,
                    stream = false
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{ollamaUrl.TrimEnd('/')}/api/generate";
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Ollama API request failed: {response.StatusCode} - {response.ReasonPhrase}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw Ollama response length: {responseJson?.Length ?? 0} chars"); // Debug logging
                
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
                
                var result = new OllamaResponse();
                
                // Extract the response text
                if (responseObj.TryGetProperty("response", out var responseProperty))
                {
                    result.Response = responseProperty.GetString() ?? "No response from Ollama";
                }
                else
                {
                    result.Response = "No response from Ollama";
                }

                // Extract performance metrics
                var metrics = new OllamaMetrics();
                bool hasAnyMetrics = false;
                
                if (responseObj.TryGetProperty("total_duration", out var totalDuration))
                {
                    metrics.TotalDuration = totalDuration.GetInt64();
                    hasAnyMetrics = true;
                }
                
                if (responseObj.TryGetProperty("load_duration", out var loadDuration))
                {
                    metrics.LoadDuration = loadDuration.GetInt64();
                    hasAnyMetrics = true;
                }
                
                if (responseObj.TryGetProperty("prompt_eval_count", out var promptEvalCount))
                {
                    metrics.PromptEvalCount = promptEvalCount.GetInt32();
                    hasAnyMetrics = true;
                }
                
                if (responseObj.TryGetProperty("prompt_eval_duration", out var promptEvalDuration))
                {
                    metrics.PromptEvalDuration = promptEvalDuration.GetInt64();
                    hasAnyMetrics = true;
                }
                
                if (responseObj.TryGetProperty("eval_count", out var evalCount))
                {
                    metrics.EvalCount = evalCount.GetInt32();
                    hasAnyMetrics = true;
                }
                
                if (responseObj.TryGetProperty("eval_duration", out var evalDuration))
                {
                    metrics.EvalDuration = evalDuration.GetInt64();
                    hasAnyMetrics = true;
                }

                // Only set metrics if we actually found some
                if (hasAnyMetrics)
                {
                    result.Metrics = metrics;
                    Console.WriteLine($"Parsed Ollama metrics - Total: {metrics.TotalDurationSeconds:F2}s, " +
                                    $"Input: {metrics.PromptTokensPerSecond:F1} tok/s, " +
                                    $"Output: {metrics.OutputTokensPerSecond:F1} tok/s");
                }
                else
                {
                    Console.WriteLine("No Ollama metrics found in response");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get code review from Ollama: {ex.Message}", ex);
            }
        }

        // Keep the old method for backward compatibility
        public async Task<string> GetCodeReviewAsync(string ollamaUrl, string prompt, string diff)
        {
            var response = await GetCodeReviewWithMetricsAsync(ollamaUrl, prompt, diff);
            return response.Response;
        }
    }
}