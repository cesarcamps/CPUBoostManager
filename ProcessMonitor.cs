using System;
using System.Diagnostics;
using System.Management;

namespace CPUBoostManager
{
    public class ProcessMonitor
    {
        private readonly GameDetector _gameDetector;
        private readonly PowerManager _powerManager;
        private ManagementEventWatcher? _processStartWatcher;
        private ManagementEventWatcher? _processStopWatcher;
        private bool _isMonitoring = false;

        public ProcessMonitor(GameDetector gameDetector, PowerManager powerManager)
        {
            _gameDetector = gameDetector;
            _powerManager = powerManager;
        }

        public void StartMonitoring()
        {
            if (_isMonitoring) return;

            try
            {
                // Set up WMI query for process start events
                var startQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'");
                _processStartWatcher = new ManagementEventWatcher(startQuery);
                _processStartWatcher.EventArrived += ProcessStarted;
                _processStartWatcher.Start();

                // Set up WMI query for process stop events
                var stopQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'");
                _processStopWatcher = new ManagementEventWatcher(stopQuery);
                _processStopWatcher.EventArrived += ProcessStopped;
                _processStopWatcher.Start();

                _isMonitoring = true;
                Console.WriteLine("Process monitoring started.");
                
                // Check for existing game processes
                CheckExistingProcesses();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting process monitoring: {ex.Message}");
                StopMonitoring();
            }
        }

        public void StopMonitoring()
        {
            if (!_isMonitoring) return;

            try
            {
                _processStartWatcher?.Stop();
                _processStopWatcher?.Stop();

                _processStartWatcher?.Dispose();
                _processStopWatcher?.Dispose();

                _processStartWatcher = null;
                _processStopWatcher = null;

                _isMonitoring = false;
                Console.WriteLine("Process monitoring stopped.");
                
                // Re-enable boost when monitoring stops
                _powerManager.EnableBoost();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping process monitoring: {ex.Message}");
            }
        }

        private void ProcessStarted(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var processInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
                int processId = Convert.ToInt32(processInstance["ProcessId"]);
                string processName = processInstance["Name"].ToString() ?? string.Empty;
                
                var process = Process.GetProcessById(processId);
                if (_gameDetector.IsGameProcess(process))
                {
                    _gameDetector.RegisterGameProcess(process);
                    _powerManager.DisableBoost();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing start event: {ex.Message}");
            }
        }

        private void ProcessStopped(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var processInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
                int processId = Convert.ToInt32(processInstance["ProcessId"]);
                
                _gameDetector.UnregisterGameProcess(processId);
                
                // If no more games are running, re-enable CPU boost
                if (!_gameDetector.HasActiveGames())
                {
                    _powerManager.EnableBoost();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing stop event: {ex.Message}");
            }
        }
        
        private void CheckExistingProcesses()
        {
            try
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        if (_gameDetector.IsGameProcess(process))
                        {
                            _gameDetector.RegisterGameProcess(process);
                            _powerManager.DisableBoost();
                            break; // Found at least one game running, so disable boost
                        }
                    }
                    catch
                    {
                        // Skip any process we can't access
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking existing processes: {ex.Message}");
            }
        }
    }
}