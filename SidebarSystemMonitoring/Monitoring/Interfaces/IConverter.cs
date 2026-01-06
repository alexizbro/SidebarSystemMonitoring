using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Interfaces;

public interface IConverter
{
    void Convert(ref double value);

    void Convert(ref double value, out double normalized, out DataType targetType);

    DataType TargetType { get; }

    bool IsDynamic { get; }
}