using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.SignalR;
using VibeShootout.Backend.Hubs;
using VibeShootout.Backend.Models;
using VibeShootout.Backend.Services;

namespace VibeShootout.Backend
{
    public class WebServer
    {
        private WebApplication? _app;
        private readonly ConfigService _configService;
        private readonly FileWatcherService _fileWatcherService;
        private IHubContext<CodeReviewHub>? _hubContext;

        public WebServer(ConfigService configService, FileWatcherService fileWatcherService)
        {
            _configService = configService;
            _fileWatcherService = fileWatcherService;
            _fileWatcherService.CodeReviewCompleted += OnCodeReviewCompleted;
        }

        public async Task StartAsync()
        {
            Console.WriteLine("Starting web server...");
            
            var builder = WebApplication.CreateBuilder();

            // Add services
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddSignalR();
            builder.Services.AddControllers();

            // Configure URLs
            builder.WebHost.UseUrls("http://localhost:5632");

            _app = builder.Build();

            // Configure pipeline
            _app.UseCors();
            
            // Configure static files to serve React app
            var clientAppPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClientApp", "build");
            Console.WriteLine($"Looking for React build at: {clientAppPath}");
            
            if (Directory.Exists(clientAppPath))
            {
                Console.WriteLine("Found React build folder, serving React app");
                _app.UseDefaultFiles(new DefaultFilesOptions
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(clientAppPath)
                });
                _app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(clientAppPath)
                });
            }
            else
            {
                Console.WriteLine("React build folder not found, serving from wwwroot");
                // Fallback to serving from wwwroot if build folder doesn't exist
                _app.UseDefaultFiles();
                _app.UseStaticFiles();
            }

            // API endpoints
            _app.MapGet("/api/config", async () =>
            {
                Console.WriteLine("GET /api/config called");
                var config = await _configService.GetConfigAsync();
                return Results.Ok(config);
            });

            _app.MapPost("/api/config", async (AppConfig config) =>
            {
                Console.WriteLine("POST /api/config called");
                await _configService.SaveConfigAsync(config);
                
                // Restart file watcher with new repository path
                try
                {
                    if (!string.IsNullOrEmpty(config.RepositoryPath))
                    {
                        await _fileWatcherService.StartWatchingAsync(config.RepositoryPath);
                    }
                    else
                    {
                        _fileWatcherService.StopWatching();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error configuring file watcher: {ex.Message}");
                    return Results.BadRequest(new { error = ex.Message });
                }

                return Results.Ok(config);
            });

            // SignalR hub
            _app.MapHub<CodeReviewHub>("/hubs/codereview");

            // Fallback to index.html for SPA
            _app.MapFallbackToFile("index.html");

            // Store hub context for later use
            _hubContext = _app.Services.GetRequiredService<IHubContext<CodeReviewHub>>();

            await _app.StartAsync();
            Console.WriteLine("Web server started successfully on http://localhost:5632");

            // Start watching if repository path is configured
            var config = await _configService.GetConfigAsync();
            if (!string.IsNullOrEmpty(config.RepositoryPath))
            {
                try
                {
                    Console.WriteLine($"Starting file watcher for: {config.RepositoryPath}");
                    await _fileWatcherService.StartWatchingAsync(config.RepositoryPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to start file watcher: {ex.Message}");
                }
            }
        }

        private async void OnCodeReviewCompleted(CodeReviewResult result)
        {
            Console.WriteLine($"Code review completed: {(result.IsSuccess ? "Success" : "Failed")}");
            if (_hubContext != null)
            {
                await _hubContext.Clients.All.SendAsync("CodeReviewCompleted", result);
            }
        }

        public async Task StopAsync()
        {
            Console.WriteLine("Stopping web server...");
            if (_app != null)
            {
                await _app.StopAsync();
                await _app.DisposeAsync();
            }
            Console.WriteLine("Web server stopped");
        }
    }
}