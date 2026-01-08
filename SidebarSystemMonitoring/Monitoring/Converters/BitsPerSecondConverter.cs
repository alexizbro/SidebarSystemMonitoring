using SidebarSystemMonitoring.Monitoring.Interfaces;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Converters;

public class BitsPerSecondConverter : IConverter
{
    private BitsPerSecondConverter() { }

    public void Convert(ref double value)
    {
        double _normalized;
        DataType _dataType;

        Convert(ref value, out _normalized, out _dataType);
    }

    public void Convert(ref double value, out double normalized, out DataType targetType)
    {
        normalized = value /= 128d;

        if (value < 1024d)
        {
            targetType = DataType.kbps;
            return;
        }
        else if (value < 1048576d)
        {
            value /= 1024d;
            targetType = DataType.Mbps;
            return;
        }
        else
        {
            value /= 1048576d;
            targetType = DataType.Gbps;
            return;
        }
    }

    public DataType TargetType
    {
        get
        {
            return DataType.kbps;
        }
    }

    public bool IsDynamic
    {
        get
        {
            return true;
        }
    }

    private static BitsPerSecondConverter _instance { get; set; } = null;

    public static BitsPerSecondConverter Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BitsPerSecondConverter();
            }

            return _instance;
        }
    }
}