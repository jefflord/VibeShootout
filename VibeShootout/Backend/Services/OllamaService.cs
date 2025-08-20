using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VibeShootout.Backend.Services
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;

        public OllamaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetCodeReviewAsync(string ollamaUrl, string prompt, string diff)
        {
            try
            {
                var fullPrompt = $"{prompt}\n\n{diff}";
                
                var requestBody = new
                {
                    model = "llama3.2",
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
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
                
                if (responseObj.TryGetProperty("response", out var responseProperty))
                {
                    return responseProperty.GetString() ?? "No response from Ollama";
                }

                return "No response from Ollama";
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get code review from Ollama: {ex.Message}", ex);
            }
        }
    }
}