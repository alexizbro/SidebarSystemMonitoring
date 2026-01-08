using SidebarSystemMonitoring.Monitoring.Interfaces;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Converters;

public class CelsiusToFahrenheitConverter : IConverter
{
    private CelsiusToFahrenheitConverter() { }

    public void Convert(ref double value)
    {
        value = value * 1.8d + 32d;
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
            return DataType.Fahrenheit;
        }
    }

    public bool IsDynamic
    {
        get
        {
            return false;
        }
    }

    private static CelsiusToFahrenheitConverter _instance { get; set; } = null;

    public static CelsiusToFahrenheitConverter Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CelsiusToFahrenheitConverter();
            }

            return _instance;
        }
    }
}