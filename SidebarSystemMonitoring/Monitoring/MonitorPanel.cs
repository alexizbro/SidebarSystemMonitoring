using System;
using System.ComponentModel;
using System.Windows.Media;
using SidebarSystemMonitoring.Monitoring.Interfaces;

namespace SidebarSystemMonitoring.Monitoring;

public class MonitorPanel : INotifyPropertyChanged, IDisposable
{
    public MonitorPanel(string title, string iconData, params IMonitor[] monitors)
    {
        IconPath = Geometry.Parse(iconData);
        Title = title;
        Monitors = monitors;
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
                foreach (IMonitor _monitor in Monitors)
                {
                    _monitor.Dispose();
                }

                _monitors = null;
                _iconPath = null;
            }

            _disposed = true;
        }
    }

    ~MonitorPanel()
    {
        Dispose(false);
    }

    public void NotifyPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private Geometry _iconPath { get; set; }

    public Geometry IconPath
    {
        get
        {
            return _iconPath;
        }
        private set
        {
            _iconPath = value;

            NotifyPropertyChanged("IconPath");
        }
    }

    private string _title { get; set; }

    public string Title
    {
        get
        {
            return _title;
        }
        private set
        {
            _title = value;

            NotifyPropertyChanged("Title");
        }
    }

    private IMonitor[] _monitors { get; set; }

    public IMonitor[] Monitors
    {
        get
        {
            return _monitors;
        }
        private set
        {
            _monitors = value;

            NotifyPropertyChanged("Monitors");
        }
    }

    private bool _disposed { get; set; } = false;
}