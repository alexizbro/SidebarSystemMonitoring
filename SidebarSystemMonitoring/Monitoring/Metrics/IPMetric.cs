using System;
using SidebarSystemMonitoring.Monitoring.Interfaces;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Metrics;

public class IPMetric : BaseMetric
{
    public IPMetric(string ipAddress, MetricKey key, DataType dataType, string label = null, bool round = false, double alertValue = 0, IConverter converter = null) : base(key, dataType, label, round, alertValue, converter)
    {
        Text = ipAddress;
    }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~IPMetric()
    {
        Dispose(false);
    }

    public override bool IsNumeric
    {
        get { return false; }
    }
}