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
        private readonly ReviewCacheService _reviewCache;
        private readonly Timer _debounceTimer;
        private bool _disposed = false;

        public event Action<CodeReviewResult>? CodeReviewCompleted;

        public FileWatcherService(GitService gitService, OllamaService ollamaService, ConfigService configService)
        {
            _gitService = gitService;
            _ollamaService = ollamaService;
            _configService = configService;
            _reviewCache = new ReviewCacheService();
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
            
            Console.WriteLine($"File watcher started for repository: {repositoryPath}");
            
            return Task.CompletedTask;
        }

        public void StopWatching()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
                Console.WriteLine("File watcher stopped");
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Ignore .git folder changes and temporary files
            if (e.FullPath.Contains("\\.git\\") || e.FullPath.EndsWith(".tmp") || e.FullPath.EndsWith("~"))
                return;

            Console.WriteLine($"File change detected: {e.ChangeType} - {e.Name}");

            // Debounce file changes (restart timer)
            _debounceTimer.Change(2000, Timeout.Infinite);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            OnFileChanged(sender, e);
        }

        private async void OnDebounceTimerElapsed(object? state)
        {
            var result = new CodeReviewResult
            {
                StartTime = DateTime.UtcNow
            };

            try
            {
                Console.WriteLine("Starting code review process...");
                
                var config = await _configService.GetConfigAsync();
                if (string.IsNullOrEmpty(config.RepositoryPath))
                {
                    Console.WriteLine("No repository path configured, skipping review");
                    return;
                }

                var diff = await _gitService.GetDiffAsync(config.RepositoryPath);
                
                if (string.IsNullOrWhiteSpace(diff))
                {
                    Console.WriteLine("No changes detected in git diff, skipping review");
                    return;
                }

                // Calculate checksum for the diff
                var diffChecksum = ChecksumService.CalculateDiffChecksum(diff);
                result.DiffChecksum = diffChecksum;

                // Check if we've already reviewed this exact diff recently
                if (_reviewCache.HasRecentReview(diffChecksum))
                {
                    Console.WriteLine($"Duplicate diff detected (checksum: {diffChecksum[..8]}...), skipping review");
                    result.EndTime = DateTime.UtcNow;
                    result.IsSuccess = false;
                    result.ErrorMessage = "Duplicate diff - review skipped to avoid redundancy";
                    result.Diff = diff;
                    
                    CodeReviewCompleted?.Invoke(result);
                    return;
                }

                Console.WriteLine($"Processing new diff (checksum: {diffChecksum[..8]}..., size: {diff.Length} chars)");

                // Add to cache before processing to prevent concurrent duplicates
                _reviewCache.AddReview(diffChecksum);

                var review = await _ollamaService.GetCodeReviewAsync(config.OllamaUrl, config.ReviewPrompt, diff);

                result.EndTime = DateTime.UtcNow;
                result.Diff = diff;
                result.Review = review;
                result.IsSuccess = true;

                Console.WriteLine($"Code review completed successfully in {result.DurationMs:F0}ms (cache size: {_reviewCache.CacheSize})");

                CodeReviewCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                result.EndTime = DateTime.UtcNow;
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;

                Console.WriteLine($"Code review failed after {result.DurationMs:F0}ms: {ex.Message}");

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