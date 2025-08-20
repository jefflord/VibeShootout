using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace VibeShootout.Backend.Services
{
    public class GitService
    {
        public async Task<string> GetDiffAsync(string repositoryPath)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "diff",
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

                if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
                {
                    throw new Exception($"Git diff failed: {error}");
                }

                return output;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute git diff: {ex.Message}", ex);
            }
        }

        public bool IsGitRepository(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return false;

            var gitDir = Path.Combine(path, ".git");
            return Directory.Exists(gitDir) || File.Exists(gitDir);
        }
    }
}