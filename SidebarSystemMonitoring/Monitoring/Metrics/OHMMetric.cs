using System;
using LibreHardwareMonitor.Hardware;
using SidebarSystemMonitoring.Monitoring.Interfaces;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Metrics;

public class OHMMetric : BaseMetric
{
    public OHMMetric(ISensor sensor, MetricKey key, DataType dataType, string label = null, bool round = false, double alertValue = 0, IConverter converter = null) : base(key, dataType, label, round, alertValue, converter)
    {
        _sensor = sensor;
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
                _sensor = null;
            }

            _disposed = true;
        }
    }

    ~OHMMetric()
    {
        Dispose(false);
    }

    public override void Update()
    {
        if (_sensor.Value.HasValue)
        {
            Update(_sensor.Value.Value);
        }
        else
        {
            Text = "No Value";
        }
    }

    private ISensor _sensor { get; set; }

    private bool _disposed { get; set; } = false;
}