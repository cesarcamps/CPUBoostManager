using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CPUBoostManager
{
    public class PowerManager
    {
        [DllImport("powrprof.dll")]
        private static extern uint PowerSetActiveScheme(IntPtr UserRootPowerKey, ref Guid SchemeGuid);
        
        [DllImport("powrprof.dll")]
        private static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, out IntPtr ActivePolicyGuid);
        
        [DllImport("powrprof.dll")]
        private static extern uint PowerWriteACValueIndex(IntPtr RootPowerKey, ref Guid SchemeGuid, 
            ref Guid SubGroupOfPowerSettingsGuid, ref Guid PowerSettingGuid, uint AcValueIndex);
            
        // Processor power management
        private static readonly Guid GUID_PROCESSOR_SUBGROUP = 
            new("54533251-82be-4824-96c1-47b60b740d00");
            
        // Processor performance boost mode
        private static readonly Guid GUID_PROCESSOR_BOOST_POLICY = 
            new("be337238-0d82-4146-a960-4f3749d470c7");
            
        // Boost values
        private const uint BOOST_DISABLED = 0;
        private const uint BOOST_EFFICIENT_AGGRESSIVE = 1; // Efficient Aggressive
        private const uint BOOST_ENABLED = 2; // Aggressive
        
        private Guid _currentScheme;
        private bool _boostEnabled = true;
        
        public PowerManager()
        {
            // Get current power scheme
            IntPtr ptr = IntPtr.Zero;
            var result = PowerGetActiveScheme(IntPtr.Zero, out ptr);
            
            if (result == 0 && ptr != IntPtr.Zero)
            {
                _currentScheme = Marshal.PtrToStructure<Guid>(ptr);
                Marshal.FreeHGlobal(ptr);
                Console.WriteLine("Power Manager initialized successfully.");
            }
            else
            {
                throw new Exception("Failed to get active power scheme.");
            }
        }
        
        public void DisableBoost()
        {
            if (!_boostEnabled) return;
            
            Console.WriteLine("Disabling CPU Boost...");
            var subgroup = GUID_PROCESSOR_SUBGROUP;
            var boost_policy = GUID_PROCESSOR_BOOST_POLICY;
            var result = PowerWriteACValueIndex(
                IntPtr.Zero,
                ref _currentScheme,
                ref subgroup,
                ref boost_policy,
                BOOST_DISABLED);
                
            if (result == 0)
            {
                PowerSetActiveScheme(IntPtr.Zero, ref _currentScheme);
                _boostEnabled = false;
                Console.WriteLine("CPU Boost disabled successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to disable CPU Boost. Error code: {result}");
            }
        }
        
        public void EnableBoost()
        {
            if (_boostEnabled) return;
            
            Console.WriteLine("Enabling CPU Boost...");
            var subgroup = GUID_PROCESSOR_SUBGROUP;
            var boost_policy = GUID_PROCESSOR_BOOST_POLICY;
            var result = PowerWriteACValueIndex(
                IntPtr.Zero,
                ref _currentScheme,
                ref subgroup,
                ref boost_policy,
                BOOST_EFFICIENT_AGGRESSIVE);
                
            if (result == 0)
            {
                PowerSetActiveScheme(IntPtr.Zero, ref _currentScheme);
                _boostEnabled = true;
                Console.WriteLine("CPU Boost enabled successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to enable CPU Boost. Error code: {result}");
            }
        }
    }
}