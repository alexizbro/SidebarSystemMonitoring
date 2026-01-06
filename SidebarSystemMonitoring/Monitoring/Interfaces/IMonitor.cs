using System;
using System.ComponentModel;

namespace SidebarSystemMonitoring.Monitoring.Interfaces;

public interface IMonitor : INotifyPropertyChanged, IDisposable
{
    string ID { get; }

    string Name { get; }

    bool ShowName { get; }
    IMetric[] Metrics { get; }

    void Update();
}