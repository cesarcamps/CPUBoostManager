using System;
using System.Threading;

namespace CPUBoostManager
{
    internal class Program
    {
        private static ProcessMonitor? _monitor;
        private static PowerManager? _powerManager;
        private static GameDetector? _gameDetector;
        private static bool _isRunning = true;

        static void Main(string[] args)
        {
            Console.WriteLine("CPU Boost Manager Starting...");
            
            try
            {
                // Initialize components
                _powerManager = new PowerManager();
                _gameDetector = new GameDetector();
                _monitor = new ProcessMonitor(_gameDetector, _powerManager);
                
                // Start monitoring
                _monitor.StartMonitoring();

                Console.WriteLine("CPU Boost Manager is now running. Press Ctrl+C to exit.");
                
                // Handle graceful shutdown
                Console.CancelKeyPress += (sender, e) => 
                {
                    e.Cancel = true;
                    _isRunning = false;
                    Shutdown();
                };
                
                // Keep the application running
                while (_isRunning)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        private static void Shutdown()
        {
            Console.WriteLine("Shutting down CPU Boost Manager...");
            _monitor?.StopMonitoring();
            Console.WriteLine("CPU Boost Manager stopped.");
        }
    }
}
