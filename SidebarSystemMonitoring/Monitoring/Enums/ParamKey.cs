using System;

namespace SidebarSystemMonitoring.Monitoring.Metrics.Enums;

[Serializable]
public enum ParamKey : byte
{
    HardwareNames,
    UseFahrenheit,
    AllCoreClocks,
    CoreLoads,
    TempAlert,
    DriveDetails,
    UsedSpaceAlert,
    BandwidthInAlert,
    BandwidthOutAlert,
    UseBytes,
    RoundAll,
    DriveSpace,
    DriveIO,
    UseGHz
}