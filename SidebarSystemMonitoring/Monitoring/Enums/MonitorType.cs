using System;

namespace SidebarSystemMonitoring.Monitoring.Metrics.Enums;

[Serializable]
public enum MonitorType : byte
{
    CPU,
    RAM,
    GPU,
    HD,
    Network
}