using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using VibeShootout.Backend.Models;

namespace VibeShootout.Backend.Services
{
    public class AIResponse
    {
        public string Response { get; set; } = "";
        public OllamaMetrics? Metrics { get; set; }
        public OpenAIUsage? Usage { get; set; }
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

    public class OpenAIUsage
    {
        [JsonPropertyName("promptTokens")]
        public int PromptTokens { get; set; }
        
        [JsonPropertyName("completionTokens")]
        public int CompletionTokens { get; set; }
        
        [JsonPropertyName("totalTokens")]
        public int TotalTokens { get; set; }
    }

    public class AIService
    {
        private readonly HttpClient _httpClient;

        public AIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AIResponse> GetCodeReviewWithMetricsAsync(AppConfig config, string prompt, string diff)
        {
            return config.Provider switch
            {
                AIProvider.Ollama => await GetOllamaReviewAsync(config, prompt, diff),
                AIProvider.OpenAI => await GetOpenAIReviewAsync(config, prompt, diff),
                _ => throw new ArgumentException($"Unsupported AI provider: {config.Provider}")
            };
        }

        private async Task<AIResponse> GetOllamaReviewAsync(AppConfig config, string prompt, string diff)
        {
            try
            {
                var fullPrompt = $"{prompt}\n\n{diff}";
                
                var requestBody = new
                {
                    model = config.OllamaModel,
                    prompt = fullPrompt,
                    stream = false
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{config.OllamaUrl.TrimEnd('/')}/api/generate";
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Ollama API request failed: {response.StatusCode} - {response.ReasonPhrase}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw Ollama response length: {responseJson?.Length ?? 0} chars");
                
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
                
                var result = new AIResponse();
                
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

        private async Task<AIResponse> GetOpenAIReviewAsync(AppConfig config, string prompt, string diff)
        {
            try
            {
                var fullPrompt = $"{prompt}\n\n{diff}";
                
                var requestBody = new
                {
                    model = config.OpenAIModel,
                    messages = new[]
                    {
                        new { role = "user", content = fullPrompt }
                    },
                    temperature = 0.7,
                    max_tokens = 4000
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add Authorization header only if API key is provided and not empty
                if (!string.IsNullOrEmpty(config.OpenAIApiKey))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.OpenAIApiKey);
                }
                else
                {
                    // Remove any existing Authorization header if no API key is provided
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }

                var url = $"{config.OpenAIUrl.TrimEnd('/')}/v1/chat/completions";
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"OpenAI API request failed: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw OpenAI response length: {responseJson?.Length ?? 0} chars");
                
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
                
                var result = new AIResponse();
                
                // Extract the response text from OpenAI's format
                if (responseObj.TryGetProperty("choices", out var choicesProperty) && 
                    choicesProperty.GetArrayLength() > 0)
                {
                    var firstChoice = choicesProperty[0];
                    if (firstChoice.TryGetProperty("message", out var messageProperty) &&
                        messageProperty.TryGetProperty("content", out var contentProperty))
                    {
                        result.Response = contentProperty.GetString() ?? "No response from OpenAI";
                    }
                    else
                    {
                        result.Response = "No response from OpenAI";
                    }
                }
                else
                {
                    result.Response = "No response from OpenAI";
                }

                // Extract usage statistics
                if (responseObj.TryGetProperty("usage", out var usageProperty))
                {
                    var usage = new OpenAIUsage();
                    
                    if (usageProperty.TryGetProperty("prompt_tokens", out var promptTokens))
                    {
                        usage.PromptTokens = promptTokens.GetInt32();
                    }
                    
                    if (usageProperty.TryGetProperty("completion_tokens", out var completionTokens))
                    {
                        usage.CompletionTokens = completionTokens.GetInt32();
                    }
                    
                    if (usageProperty.TryGetProperty("total_tokens", out var totalTokens))
                    {
                        usage.TotalTokens = totalTokens.GetInt32();
                    }
                    
                    result.Usage = usage;
                    Console.WriteLine($"OpenAI usage - Prompt: {usage.PromptTokens} tokens, " +
                                    $"Completion: {usage.CompletionTokens} tokens, " +
                                    $"Total: {usage.TotalTokens} tokens");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get code review from OpenAI: {ex.Message}", ex);
            }
        }

        // Keep backward compatibility with old method name
        public async Task<string> GetCodeReviewAsync(string ollamaUrl, string prompt, string diff)
        {
            var config = new AppConfig { OllamaUrl = ollamaUrl, Provider = AIProvider.Ollama };
            var response = await GetCodeReviewWithMetricsAsync(config, prompt, diff);
            return response.Response;
        }

        // New method for compatibility with existing code that expects OllamaResponse
        public async Task<OllamaResponse> GetCodeReviewWithMetricsAsync(string ollamaUrl, string prompt, string diff)
        {
            var config = new AppConfig { OllamaUrl = ollamaUrl, Provider = AIProvider.Ollama };
            var response = await GetCodeReviewWithMetricsAsync(config, prompt, diff);
            return new OllamaResponse
            {
                Response = response.Response,
                Metrics = response.Metrics
            };
        }
    }

    // Keep OllamaResponse and OllamaService for backward compatibility
    public class OllamaResponse
    {
        public string Response { get; set; } = "";
        public OllamaMetrics? Metrics { get; set; }
    }

    public class OllamaService : AIService
    {
        public OllamaService(HttpClient httpClient) : base(httpClient)
        {
        }
    }
}