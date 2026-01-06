using System;
using System.ComponentModel;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Interfaces;

public interface IMetric : INotifyPropertyChanged, IDisposable
{
    MetricKey Key { get; }

    string FullName { get; }

    string Label { get; }

    double Value { get; }

    string Append { get; }

    double nValue { get; }

    string nAppend { get; }

    string Text { get; }

    bool IsAlert { get; }

    bool IsNumeric { get; }

    void Update();

    void Update(double value);
}
