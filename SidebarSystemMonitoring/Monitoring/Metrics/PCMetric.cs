using System;
using System.Diagnostics;
using SidebarSystemMonitoring.Monitoring.Interfaces;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Metrics;

public class PCMetric : BaseMetric
{
    public PCMetric(PerformanceCounter counter, MetricKey key, DataType dataType, string label = null, bool round = false, double alertValue = 0, IConverter converter = null) : base(key, dataType, label, round, alertValue, converter)
    {
        _counter = counter;
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
                if (_counter != null)
                {
                    _counter.Dispose();
                    _counter = null;
                }
            }

            _disposed = true;
        }
    }

    ~PCMetric()
    {
        Dispose(false);
    }

    public override void Update()
    {
        Update(_counter.NextValue());
    }

    private PerformanceCounter _counter { get; set; }

    private bool _disposed { get; set; } = false;
}