using System;
using LibreHardwareMonitor.Hardware;
using SidebarSystemMonitoring.Monitoring.Interfaces;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Metrics;

public class GpuVramLoadMetric : BaseMetric
{
    public GpuVramLoadMetric(ISensor memoryUsedSensor, ISensor memoryTotalSensor, MetricKey key, DataType dataType, string label = null, bool round = false, double alertValue = 0, IConverter converter = null) : base(key, dataType, label, round, alertValue, converter)
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

    ~GpuVramLoadMetric()
    {
        Dispose(false);
    }

    public override void Update()
    {
        if (_memoryUsedSensor.Value.HasValue && _memoryTotalSensor.Value.HasValue)
        {
            float load = _memoryUsedSensor.Value.Value / _memoryTotalSensor.Value.Value * 100f;

            Update(load);
        }
        else
        {
            Text = "No Value";
        }
    }

    private ISensor _memoryUsedSensor { get; set; }

    private ISensor _memoryTotalSensor { get; set; }

    private bool _disposed { get; set; } = false;
}