using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using Microsoft.AspNetCore.StaticFiles;

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
                    policy.WithOrigins("http://localhost:5632", "http://127.0.0.1:5632", "http://127.0.0.1:5632 FUCK")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Important for SignalR
                });
            });

            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true; // For debugging
            });
            
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
            });
            
            builder.Services.AddControllers();

            // Configure URLs
            builder.WebHost.UseUrls("http://localhost:5632");

            _app = builder.Build();

            // Configure pipeline
            _app.UseCors();
            
            // Configure static files to serve React app
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine($"Base directory: {baseDirectory}");
            
            var clientAppPath = Path.Combine(baseDirectory, "ClientApp", "build");
            Console.WriteLine($"Looking for React build at: {clientAppPath}");
            Console.WriteLine($"Directory exists: {Directory.Exists(clientAppPath)}");
            
            PhysicalFileProvider? reactFileProvider = null;
            
            // Create content type provider with proper UTF-8 encoding
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".js"] = "application/javascript; charset=utf-8";
            contentTypeProvider.Mappings[".html"] = "text/html; charset=utf-8";
            contentTypeProvider.Mappings[".css"] = "text/css; charset=utf-8";
            contentTypeProvider.Mappings[".json"] = "application/json; charset=utf-8";
            
            if (Directory.Exists(clientAppPath))
            {
                Console.WriteLine("Found React build folder, serving React app");
                var files = Directory.GetFiles(clientAppPath, "*", SearchOption.AllDirectories);
                Console.WriteLine($"Found {files.Length} files in React build");
                foreach (var file in files.Take(5)) // Show first 5 files
                {
                    Console.WriteLine($"  - {Path.GetRelativePath(clientAppPath, file)}");
                }
                
                reactFileProvider = new PhysicalFileProvider(clientAppPath);
                
                // Configure static files FIRST (before default files)
                _app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = reactFileProvider,
                    RequestPath = "", // Serve from root
                    ContentTypeProvider = contentTypeProvider,
                    OnPrepareResponse = ctx =>
                    {
                        // Ensure UTF-8 encoding for text files
                        var contentType = ctx.Context.Response.ContentType;
                        if (!string.IsNullOrEmpty(contentType) && 
                            (contentType.StartsWith("text/") || 
                             contentType.StartsWith("application/javascript") ||
                             contentType.StartsWith("application/json")))
                        {
                            if (!contentType.Contains("charset"))
                            {
                                ctx.Context.Response.ContentType = contentType + "; charset=utf-8";
                            }
                        }
                    }
                });
                
                Console.WriteLine("Static file middleware configured for React app with UTF-8 encoding");
            }
            else
            {
                Console.WriteLine("React build folder not found, serving from wwwroot");
                var wwwrootPath = Path.Combine(baseDirectory, "wwwroot");
                Console.WriteLine($"Wwwroot path: {wwwrootPath}");
                Console.WriteLine($"Wwwroot exists: {Directory.Exists(wwwrootPath)}");
                
                // Fallback to serving from wwwroot if build folder doesn't exist
                _app.UseStaticFiles(new StaticFileOptions
                {
                    ContentTypeProvider = contentTypeProvider,
                    OnPrepareResponse = ctx =>
                    {
                        // Ensure UTF-8 encoding for text files
                        var contentType = ctx.Context.Response.ContentType;
                        if (!string.IsNullOrEmpty(contentType) && 
                            (contentType.StartsWith("text/") || 
                             contentType.StartsWith("application/javascript") ||
                             contentType.StartsWith("application/json")))
                        {
                            if (!contentType.Contains("charset"))
                            {
                                ctx.Context.Response.ContentType = contentType + "; charset=utf-8";
                            }
                        }
                    }
                });
                Console.WriteLine("Static file middleware configured for wwwroot with UTF-8 encoding");
            }

            // API endpoints (these take priority over fallback)
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
                        Console.WriteLine($"File watcher started for: {config.RepositoryPath}");
                    }
                    else
                    {
                        _fileWatcherService.StopWatching();
                        Console.WriteLine("File watcher stopped");
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

            // SPA fallback - this should serve index.html for any unmatched routes
            if (reactFileProvider != null)
            {
                // Use the React app's index.html for SPA fallback
                _app.MapFallbackToFile("index.html", new StaticFileOptions
                {
                    FileProvider = reactFileProvider,
                    ContentTypeProvider = contentTypeProvider
                });
                Console.WriteLine("SPA fallback configured for React app");
            }
            else
            {
                // Fallback to wwwroot index.html
                _app.MapFallbackToFile("index.html", new StaticFileOptions
                {
                    ContentTypeProvider = contentTypeProvider
                });
                Console.WriteLine("SPA fallback configured for wwwroot");
            }

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
            else
            {
                Console.WriteLine("No repository path configured, file watcher not started");
            }
        }

        private async void OnCodeReviewCompleted(CodeReviewResult result)
        {
            Console.WriteLine($"Code review completed: {(result.IsSuccess ? "Success" : "Failed")}");
            Console.WriteLine($"Review text length: {result.Review?.Length ?? 0}");
            
            if (_hubContext != null)
            {
                try
                {
                    Console.WriteLine("Sending CodeReviewCompleted to all SignalR clients...");
                    await _hubContext.Clients.All.SendAsync("CodeReviewCompleted", result);
                    Console.WriteLine("SignalR message sent successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send SignalR message: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("SignalR hub context is null - cannot send message");
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