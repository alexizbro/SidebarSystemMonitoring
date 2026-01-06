using SidebarSystemMonitoring.Monitoring.Interfaces;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Converters;

public class MegaHertzToGigaHertzConverter : IConverter
{
    private MegaHertzToGigaHertzConverter() { }

    public void Convert(ref double value)
    {
        value = value / 1000d;
    }

    public void Convert(ref double value, out double normalized, out DataType targetType)
    {
        Convert(ref value);
        normalized = value;
        targetType = TargetType;
    }

    public DataType TargetType
    {
        get
        {
            return DataType.GHz;
        }
    }

    public bool IsDynamic
    {
        get
        {
            return false;
        }
    }

    private static MegaHertzToGigaHertzConverter _instance { get; set; } = null;

    public static MegaHertzToGigaHertzConverter Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MegaHertzToGigaHertzConverter();
            }

            return _instance;
        }
    }
}