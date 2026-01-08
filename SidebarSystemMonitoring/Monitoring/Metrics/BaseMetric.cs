using System;
using System.ComponentModel;
using System.Windows.Threading;
using SidebarSystemMonitoring.Monitoring.Interfaces;
using SidebarSystemMonitoring.Monitoring.Metrics.Enums;

namespace SidebarSystemMonitoring.Monitoring.Metrics;

public class BaseMetric : IMetric
{
    public BaseMetric(MetricKey key, DataType dataType, string label = null, bool round = false, double alertValue = 0, IConverter converter = null)
    {
        _converter = converter;
        _round = round;
        _alertValue = alertValue;

        Key = key;

        if (label == null)
        {
            FullName = key.GetFullName();
            Label = key.GetLabel();
        }
        else
        {
            FullName = Label = label;
        }

        nAppend = Append = converter == null ? dataType.GetAppend() : converter.TargetType.GetAppend();
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
                if (_alertColorTimer != null)
                {
                    _alertColorTimer.Stop();
                    _alertColorTimer = null;
                }

                _converter = null;
            }

            _disposed = true;
        }
    }

    ~BaseMetric()
    {
        Dispose(false);
    }

    public virtual void Update() { }

    public void Update(double value)
    {
        double _val = value;

        if (_converter == null)
        {
            nValue = _val;
        }
        else if (_converter.IsDynamic)
        {
            double _nVal;
            DataType _dataType;

            _converter.Convert(ref _val, out _nVal, out _dataType);

            nValue = _nVal;
            Append = _dataType.GetAppend();
        }
        else
        {
            _converter.Convert(ref _val);

            nValue = _val;
        }

        Value = _val;

        if (_alertValue > 0 && _alertValue <= nValue)
        {
            if (!IsAlert)
            {
                IsAlert = true;
            }
        }
        else if (IsAlert)
        {
            IsAlert = false;
        }

        Text = string.Format(
            "{0:#,##0.##}{1}",
            _val.Round(_round),
            Append
            );
    }

    public void NotifyPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private MetricKey _key { get; set; }

    public MetricKey Key
    {
        get
        {
            return _key;
        }
        protected set
        {
            _key = value;

            NotifyPropertyChanged("Key");
        }
    }

    private string _fullName { get; set; }

    public string FullName
    {
        get
        {
            return _fullName;
        }
        protected set
        {
            _fullName = value;

            NotifyPropertyChanged("FullName");
        }
    }

    private string _label { get; set; }

    public string Label
    {
        get
        {
            return _label;
        }
        protected set
        {
            _label = value;

            NotifyPropertyChanged("Label");
        }
    }

    private double _value { get; set; }

    public double Value
    {
        get
        {
            return _value;
        }
        protected set
        {
            _value = value;

            NotifyPropertyChanged("Value");
        }
    }

    private string _append { get; set; }

    public string Append
    {
        get
        {
            return _append;
        }
        protected set
        {
            _append = value;

            NotifyPropertyChanged("Append");
        }
    }

    private double _nValue { get; set; }

    public double nValue
    {
        get
        {
            return _nValue;
        }
        set
        {
            _nValue = value;

            NotifyPropertyChanged("nValue");
        }
    }

    private string _nAppend { get; set; }

    public string nAppend
    {
        get
        {
            return _nAppend;
        }
        set
        {
            _nAppend = value;

            NotifyPropertyChanged("nAppend");
        }
    }

    private string _text { get; set; }

    public string Text
    {
        get
        {
            return _text;
        }
        protected set
        {
            _text = value;

            NotifyPropertyChanged("Text");
        }
    }

    private bool _isAlert { get; set; }

    public bool IsAlert
    {
        get
        {
            return _isAlert;
        }
        protected set
        {
            _isAlert = value;

            NotifyPropertyChanged("IsAlert");

            if (value)
            {
                _alertColorFlag = false;

                if (Framework.Settings.Instance.AlertBlink)
                {
                    _alertColorTimer = new DispatcherTimer(DispatcherPriority.Normal, App.Current.Dispatcher);
                    _alertColorTimer.Interval = TimeSpan.FromSeconds(0.5d);
                    _alertColorTimer.Tick += new EventHandler(AlertColorTimer_Tick);
                    _alertColorTimer.Start();
                }
            }
            else if (_alertColorTimer != null)
            {
                _alertColorTimer.Stop();
                _alertColorTimer = null;
            }
        }
    }

    public virtual bool IsNumeric
    {
        get { return true; }
    }

    public string AlertColor
    {
        get
        {
            return _alertColorFlag ? Framework.Settings.Instance.FontColor : Framework.Settings.Instance.AlertFontColor;
        }
    }

    private DispatcherTimer _alertColorTimer;

    private void AlertColorTimer_Tick(object sender, EventArgs e)
    {
        _alertColorFlag = !_alertColorFlag;

        NotifyPropertyChanged("AlertColor");
    }

    private bool _alertColorFlag = false;

    protected IConverter _converter { get; set; }

    protected bool _round { get; set; }

    protected double _alertValue { get; set; }

    private bool _disposed { get; set; } = false;
}