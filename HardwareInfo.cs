using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreHWM_MQTT
{
    public class HardwareInfo
    {
        public float cpuTemp { get; set; }
        // CPU Usage
        public float cpuUsage { get; set; }
        // CPU Power Draw (Package)
        public float cpuPowerDrawPackage { get; set; }
        // CPU Frequency
        public float cpuFrequency { get; set; }
        // GPU Temperature
        public float gpuTemp { get; set; }
        // GPU Usage
        public float gpuUsage { get; set; }
        // GPU Core Frequency
        public float gpuCoreFrequency { get; set; } 
        // GPU Memory Frequency
        public float gpuMemoryFrequency { get; set; }
    }
}
