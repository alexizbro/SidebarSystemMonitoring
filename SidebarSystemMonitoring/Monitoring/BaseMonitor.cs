using System;
using System.ComponentModel;
using SidebarSystemMonitoring.Monitoring.Interfaces;

namespace SidebarSystemMonitoring.Monitoring;

public class BaseMonitor : IMonitor
{
    public BaseMonitor(string id, string name, bool showName)
    {
        ID = id;
        Name = name;
        ShowName = showName;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                foreach (IMetric _metric in Metrics)
                {
                    _metric.Dispose();
                }

                _metrics = null;
            }

            _disposed = true;
        }
    }

    ~BaseMonitor()
    {
        Dispose(false);
    }

    public virtual void Update()
    {
        foreach (IMetric _metric in Metrics)
        {
            _metric.Update();
        }
    }

    public void NotifyPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private string _id { get; set; }

    public string ID
    {
        get
        {
            return _id;
        }
        protected set
        {
            _id = value;

            NotifyPropertyChanged("ID");
        }
    }

    private string _name { get; set; }

    public string Name
    {
        get
        {
            return _name;
        }
        protected set
        {
            _name = value;

            NotifyPropertyChanged("Name");
        }
    }

    private bool _showName { get; set; }

    public bool ShowName
    {
        get
        {
            return _showName;
        }
        protected set
        {
            _showName = value;

            NotifyPropertyChanged("ShowName");
        }
    }

    private IMetric[] _metrics { get; set; }

    public IMetric[] Metrics
    {
        get
        {
            return _metrics;
        }
        protected set
        {
            _metrics = value;

            NotifyPropertyChanged("Metrics");
        }
    }

    private bool _disposed { get; set; } = false;
}