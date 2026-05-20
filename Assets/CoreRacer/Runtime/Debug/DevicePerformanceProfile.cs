using System;
using UnityEngine;

namespace CoreRacer.Debugging
{
    [Serializable]
    public sealed class DevicePerformanceProfile
    {
        public string DeviceModel;
        public string GpuName;
        public int SystemMemoryMb;
        public int GraphicsMemoryMb;
        public int ProcessorCount;

        public static DevicePerformanceProfile Capture()
        {
            return new DevicePerformanceProfile
            {
                DeviceModel = SystemInfo.deviceModel,
                GpuName = SystemInfo.graphicsDeviceName,
                SystemMemoryMb = SystemInfo.systemMemorySize,
                GraphicsMemoryMb = SystemInfo.graphicsMemorySize,
                ProcessorCount = SystemInfo.processorCount
            };
        }
    }
}
