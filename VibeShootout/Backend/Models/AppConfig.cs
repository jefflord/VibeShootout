namespace VibeShootout.Backend.Models
{
    public class AppConfig
    {
        public string OllamaUrl { get; set; } = "http://10.0.0.90:11434";
        public string ReviewPrompt { get; set; } = "Below is git diff, please create a code review using this information";
        public string RepositoryPath { get; set; } = "";
    }
}