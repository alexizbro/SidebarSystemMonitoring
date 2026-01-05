using System;

namespace SidebarSystemMonitoring.Models.Records;

public class MetricRecord
{
    public MetricRecord(double value, DateTime recorded)
    {
        Value = value > 0 ? value : 0.001d;
        Recorded = recorded;
    }

    public double Value { get; set; }

    public DateTime Recorded { get; set; }
}