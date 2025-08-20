using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VibeShootout.Backend.Models;

namespace VibeShootout.Backend.Services
{
    public class FileWatcherService : IDisposable
    {
        private FileSystemWatcher? _watcher;
        private readonly GitService _gitService;
        private readonly OllamaService _ollamaService;
        private readonly ConfigService _configService;
        private readonly Timer _debounceTimer;
        private bool _disposed = false;

        public event Action<CodeReviewResult>? CodeReviewCompleted;

        public FileWatcherService(GitService gitService, OllamaService ollamaService, ConfigService configService)
        {
            _gitService = gitService;
            _ollamaService = ollamaService;
            _configService = configService;
            _debounceTimer = new Timer(OnDebounceTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        public Task StartWatchingAsync(string repositoryPath)
        {
            if (string.IsNullOrEmpty(repositoryPath) || !_gitService.IsGitRepository(repositoryPath))
            {
                throw new ArgumentException("Invalid repository path", nameof(repositoryPath));
            }

            StopWatching();

            _watcher = new FileSystemWatcher(repositoryPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName
            };

            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Deleted += OnFileChanged;
            _watcher.Renamed += OnFileRenamed;

            _watcher.EnableRaisingEvents = true;
            
            return Task.CompletedTask;
        }

        public void StopWatching()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Ignore .git folder changes and temporary files
            if (e.FullPath.Contains("\\.git\\") || e.FullPath.EndsWith(".tmp") || e.FullPath.EndsWith("~"))
                return;

            // Debounce file changes (restart timer)
            _debounceTimer.Change(2000, Timeout.Infinite);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            OnFileChanged(sender, e);
        }

        private async void OnDebounceTimerElapsed(object? state)
        {
            try
            {
                var config = await _configService.GetConfigAsync();
                if (string.IsNullOrEmpty(config.RepositoryPath))
                    return;

                var diff = await _gitService.GetDiffAsync(config.RepositoryPath);
                
                if (string.IsNullOrWhiteSpace(diff))
                {
                    // No changes to review
                    return;
                }

                var review = await _ollamaService.GetCodeReviewAsync(config.OllamaUrl, config.ReviewPrompt, diff);

                var result = new CodeReviewResult
                {
                    Diff = diff,
                    Review = review,
                    IsSuccess = true
                };

                CodeReviewCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                var result = new CodeReviewResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };

                CodeReviewCompleted?.Invoke(result);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopWatching();
                _debounceTimer?.Dispose();
                _disposed = true;
            }
        }
    }
}