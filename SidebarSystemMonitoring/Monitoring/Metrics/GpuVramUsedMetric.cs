using System;
using LibreHardwareMonitor.Hardware;
using SidebarSystemMonitoring.Converters;
using SidebarSystemMonitoring.Monitoring.Interfaces;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Metrics;

public class GpuVramUsedMetric : BaseMetric
{
    private ISensor _memoryUsedSensor { get; set; }
    private bool _disposed { get; set; } = false;
    
    public GpuVramUsedMetric(
        ISensor memoryUsedSensor,
        MetricKey key,
        DataType dataType,
        string label = null,
        bool round = false,
        double alertValue = 0,
        IConverter converter = null
        ) : base(key, dataType, label, round, alertValue, converter)
    {
        _memoryUsedSensor = memoryUsedSensor;
    }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!_disposed)
        {
            if (disposing)
            {
                _memoryUsedSensor = null;
            }

            _disposed = true;
        }
    }

    ~GpuVramUsedMetric()
    {
        Dispose(false);
    }

    public override void Update()
    {
        if (_memoryUsedSensor.Value.HasValue)
        {
            Update(MegabytesToGigabytesConverter.ConvertMegabytesToGigabytes(_memoryUsedSensor.Value.Value));
        }
        else
        {
            Text = "No Value";
        }
    }
}