namespace VibeShootout.Backend.Models
{
    public enum AIProvider
    {
        Ollama,
        OpenAI
    }

    public class AppConfig
    {
        public AIProvider Provider { get; set; } = AIProvider.Ollama;
        public string OllamaUrl { get; set; } = "http://10.0.0.90:11434";
        public string OpenAIUrl { get; set; } = "http://10.0.0.90:1234";
        public string OpenAIApiKey { get; set; } = "";
        public string OpenAIModel { get; set; } = "openai/gpt-oss-20b";
        public string OllamaModel { get; set; } = "gpt-oss:20b";
        public string ReviewPrompt { get; set; } = "Below is git diff, please create a code review using this information";
        public string RepositoryPath { get; set; } = "";
    }
}