using System;

namespace CPUBoostManager.Models
{
    public class GameProcess
    {
        public int ProcessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
    }
}