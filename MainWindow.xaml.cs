using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using VibeShootout.Backend;
using VibeShootout.Backend.Services;

namespace VibeShootout
{
    public partial class MainWindow : Window
    {
        private readonly WebServer _webServer;
        private readonly FileWatcherService _fileWatcherService;

        public MainWindow()
        {
            try
            {
                Console.WriteLine("MainWindow constructor called - initializing services...");
                InitializeComponent();
                Console.WriteLine("InitializeComponent completed");
                
                // Initialize services
                Console.WriteLine("Creating ConfigService...");
                var configService = new ConfigService();
                
                Console.WriteLine("Creating GitService...");
                var gitService = new GitService();
                
                Console.WriteLine("Creating HttpClient...");
                var httpClient = new HttpClient();
                
                Console.WriteLine("Creating AIService...");
                var aiService = new AIService(httpClient);
                
                Console.WriteLine("Creating FileWatcherService...");
                _fileWatcherService = new FileWatcherService(gitService, aiService, configService);
                
                Console.WriteLine("Creating WebServer...");
                _webServer = new WebServer(configService, _fileWatcherService);

                Console.WriteLine("WebServer instance created successfully");

                Loaded += MainWindow_Loaded;
                Closing += MainWindow_Closing;
                
                Console.WriteLine("MainWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MainWindow constructor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Failed to initialize MainWindow: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("MainWindow_Loaded event fired");
                
                // Show loading message
                Title = "VibeShootout - Starting...";

                Console.WriteLine("Starting web server...");
                // Start web server first
                await _webServer.StartAsync();
                
                // Give the server a moment to fully start
                Console.WriteLine("Waiting for server to stabilize...");
                await Task.Delay(2000);

                Console.WriteLine("Checking WebView2 availability...");
                
                // Check if WebView2 runtime is available
                try
                {
                    var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                    Console.WriteLine($"WebView2 runtime version: {version}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WebView2 runtime not available: {ex.Message}");
                    MessageBox.Show("WebView2 runtime is not installed. Please install Microsoft Edge WebView2 Runtime.", 
                        "WebView2 Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Console.WriteLine("Initializing WebView2...");
                // Initialize WebView2
                await webView.EnsureCoreWebView2Async();
                
                Console.WriteLine("Configuring WebView2 settings...");
                // Configure WebView2 settings
                webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                webView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                
                // Navigate to the React app
                Console.WriteLine("Navigating WebView2 to React app at http://localhost:5632");
                webView.CoreWebView2.Navigate("http://localhost:5632");
                
                // Update title when navigation completes
                webView.CoreWebView2.NavigationCompleted += (s, args) =>
                {
                    if (args.IsSuccess)
                    {
                        Console.WriteLine("WebView2 navigation completed successfully");
                        Title = "VibeShootout - AI Code Reviewer";
                    }
                    else
                    {
                        Console.WriteLine($"WebView2 navigation failed. WebErrorStatus: {args.WebErrorStatus}");
                        Title = "VibeShootout - Navigation Failed";
                    }
                };
                
                Console.WriteLine("MainWindow_Loaded completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MainWindow_Loaded: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Failed to start application: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            try
            {
                Console.WriteLine("MainWindow closing - cleaning up...");
                _fileWatcherService?.Dispose();
                await _webServer.StopAsync();
                Console.WriteLine("Cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }
    }
}