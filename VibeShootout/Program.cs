using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace VibeShootout
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        [STAThread]
        static void Main(string[] args)
        {

            if (!true)
            {

                var app = new App();
                app.Run();

                var mainWindow = new MainWindow();

                var manWindow = new MainWindow();

                app.MainWindow = mainWindow;
                mainWindow.Show();

                return;
            }
            else
            {
                // Allocate a console for this GUI application
                AllocConsole();

                try
                {
                    Console.WriteLine("Starting VibeShootout application...");
                    Console.WriteLine("Creating WPF App instance...");

                    var app = new App();

                    // Add unhandled exception handlers
                    app.DispatcherUnhandledException += (sender, e) =>
                    {
                        Console.WriteLine($"Unhandled exception: {e.Exception.Message}");
                        Console.WriteLine($"Stack trace: {e.Exception.StackTrace}");
                        MessageBox.Show($"Application error: {e.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Handled = true;
                    };

                    AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                    {
                        Console.WriteLine($"Unhandled domain exception: {e.ExceptionObject}");
                    };

                    Console.WriteLine("Creating MainWindow manually...");
                    var mainWindow = new MainWindow();

                    Console.WriteLine("Setting MainWindow as application MainWindow...");
                    app.MainWindow = mainWindow;


                    var i = int.Parse("0") / int.Parse("234234234");

                    Console.WriteLine("Showing MainWindow...");
                    mainWindow.Show();

                    Console.WriteLine("Running WPF message loop...");
                    app.Run();

                    Console.WriteLine("VibeShootout application has closed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fatal error in Main: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    MessageBox.Show($"Fatal application error: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Console.WriteLine("Press any key to close console...");
                    Console.ReadKey();
                    FreeConsole();
                }
            }



        }
    }
}
