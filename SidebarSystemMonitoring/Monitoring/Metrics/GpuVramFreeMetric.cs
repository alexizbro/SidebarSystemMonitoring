using System;
using LibreHardwareMonitor.Hardware;
using SidebarSystemMonitoring.Converters;
using SidebarSystemMonitoring.Monitoring.Interfaces;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Metrics;

public class GpuVramFreeMetric : BaseMetric
{
    private ISensor _memoryUsedSensor { get; set; }
    private ISensor _memoryTotalSensor { get; set; }
    private bool _disposed { get; set; } = false;
    
    public GpuVramFreeMetric(
        ISensor memoryUsedSensor,
        ISensor memoryTotalSensor,
        MetricKey key,
        DataType dataType,
        string label = null,
        bool round = false,
        double alertValue = 0,
        IConverter converter = null
    ) : base(key, dataType, label, round, alertValue, converter)
    {
        _memoryUsedSensor = memoryUsedSensor;
        _memoryTotalSensor = memoryTotalSensor;
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
                _memoryTotalSensor = null;
            }

            _disposed = true;
        }
    }

    ~GpuVramFreeMetric()
    {
        Dispose(false);
    }

    public override void Update()
    {
        if (_memoryUsedSensor.Value.HasValue && _memoryTotalSensor.Value.HasValue)
        {
            Update(MegabytesToGigabytesConverter.ConvertMegabytesToGigabytes(_memoryTotalSensor.Value.Value - _memoryUsedSensor.Value.Value));
        }
        else
        {
            Text = "No Value";
        }
    }
}