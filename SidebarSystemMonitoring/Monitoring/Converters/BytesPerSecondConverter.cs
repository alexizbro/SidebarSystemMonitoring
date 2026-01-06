using SidebarSystemMonitoring.Monitoring.Interfaces;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Converters;

public class BytesPerSecondConverter : IConverter
{
    private BytesPerSecondConverter() { }

    public void Convert(ref double value)
    {
        double _normalized;
        DataType _dataType;

        Convert(ref value, out _normalized, out _dataType);
    }

    public void Convert(ref double value, out double normalized, out DataType targetType)
    {
        normalized = value /= 1024d;

        if (value < 1024d)
        {
            targetType = DataType.kBps;
            return;
        }
        else if (value < 1048576d)
        {
            value /= 1024d;
            targetType = DataType.MBps;
            return;
        }
        else
        {
            value /= 1048576d;
            targetType = DataType.GBps;
            return;
        }
    }

    public DataType TargetType
    {
        get
        {
            return DataType.kBps;
        }
    }

    public bool IsDynamic
    {
        get
        {
            return true;
        }
    }

    private static BytesPerSecondConverter _instance { get; set; } = null;

    public static BytesPerSecondConverter Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BytesPerSecondConverter();
            }

            return _instance;
        }
    }
}