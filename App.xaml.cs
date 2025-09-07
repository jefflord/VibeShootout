using System;
using System.Threading;
using System.Windows;

namespace VibeShootout
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine("App.OnStartup called (should not create MainWindow here)");
            // Don't call base.OnStartup to avoid automatic window creation
            // Don't create MainWindow here since it's done in Program.Main
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("App.OnExit called");
            base.OnExit(e);
        }
    }
}