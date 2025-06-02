using System.Collections.Generic;

namespace CPUBoostManager.Config
{
    public class AppSettings
    {
        // Additional game processes to monitor
        public List<string> AdditionalGameProcesses { get; set; } = new();
        
        // Additional game directories to monitor
        public List<string> AdditionalGameDirectories { get; set; } = new();
        
        // Time in milliseconds to wait after a game closes before re-enabling boost
        public int ReenableDelayMs { get; set; } = 2000;
        
        // Whether to show notifications when boost state changes
        public bool ShowNotifications { get; set; } = true;
        
        // Whether to start with Windows
        public bool StartWithWindows { get; set; } = false;
    }
}