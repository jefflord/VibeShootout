using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VibeShootout.Backend.Services
{
    public class GitService
    {
        public async Task<string> GetDiffAsync(string repositoryPath)
        {
            try
            {
                // Get staged changes first (if any)
                var stagedDiff = await ExecuteGitCommandAsync(repositoryPath, "diff --cached HEAD");
                
                // Get unstaged changes for tracked files only
                var unstagedDiff = await ExecuteGitCommandAsync(repositoryPath, "diff HEAD");
                
                // Combine both diffs
                var combinedDiff = string.Empty;
                
                if (!string.IsNullOrWhiteSpace(stagedDiff))
                {
                    combinedDiff += "# Staged Changes:\n" + stagedDiff + "\n\n";
                }
                
                if (!string.IsNullOrWhiteSpace(unstagedDiff))
                {
                    combinedDiff += "# Unstaged Changes:\n" + unstagedDiff;
                }
                
                // If no staged or unstaged changes, check for new tracked files
                if (string.IsNullOrWhiteSpace(combinedDiff))
                {
                    // Get diff of all changes since last commit (including new files that were added)
                    var allChangesDiff = await ExecuteGitCommandAsync(repositoryPath, "diff --no-index --name-status HEAD");
                    if (!string.IsNullOrWhiteSpace(allChangesDiff))
                    {
                        // Get the actual diff for changed tracked files
                        var trackedFilesDiff = await ExecuteGitCommandAsync(repositoryPath, "diff HEAD --");
                        if (!string.IsNullOrWhiteSpace(trackedFilesDiff))
                        {
                            combinedDiff = trackedFilesDiff;
                        }
                    }
                }

                return combinedDiff.Trim();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute git diff: {ex.Message}", ex);
            }
        }

        public async Task<string[]> GetTrackedFilesAsync(string repositoryPath)
        {
            try
            {
                var output = await ExecuteGitCommandAsync(repositoryPath, "ls-files");
                return output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get tracked files: {ex.Message}", ex);
            }
        }

        public async Task<string[]> GetModifiedTrackedFilesAsync(string repositoryPath)
        {
            try
            {
                // Get list of modified tracked files
                var output = await ExecuteGitCommandAsync(repositoryPath, "diff --name-only HEAD");
                return output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get modified tracked files: {ex.Message}", ex);
            }
        }

        private async Task<string> ExecuteGitCommandAsync(string repositoryPath, string arguments)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = repositoryPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return "";

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            // Exit code 1 is normal for git diff when there are no differences
            if (process.ExitCode != 0 && process.ExitCode != 1 && !string.IsNullOrEmpty(error))
            {
                throw new Exception($"Git command failed: {error}");
            }

            return output;
        }

        public bool IsGitRepository(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return false;

            var gitDir = Path.Combine(path, ".git");
            return Directory.Exists(gitDir) || File.Exists(gitDir);
        }

        public async Task<bool> HasTrackedChangesAsync(string repositoryPath)
        {
            try
            {
                var modifiedFiles = await GetModifiedTrackedFilesAsync(repositoryPath);
                return modifiedFiles.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}