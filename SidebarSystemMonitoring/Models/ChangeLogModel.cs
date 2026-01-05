using System;
using System.ComponentModel;
using System.Linq;
using SidebarSystemMonitoring.Framework;
using SidebarSystemMonitoring.Models.Entries;

namespace SidebarSystemMonitoring.Models
{
    public class ChangeLogModel : INotifyPropertyChanged
    {
        public ChangeLogModel(Version version)
        {
            string _vstring = version.ToString(3);

            Title = string.Format("{0} v{1}", Resources.ChangeLogTitle, _vstring);

            ChangeLogEntry _log = ChangeLogEntry.Load().FirstOrDefault(e => string.Equals(e.Version, _vstring, StringComparison.OrdinalIgnoreCase));

            if (_log != null)
            {
                Changes = _log.Changes;
            }
            else
            {
                Changes = new string[0];
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

        private string _title { get; set; }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;

                NotifyPropertyChanged("Title");
            }
        }

        private string[] _changes { get; set; }

        public string[] Changes
        {
            get
            {
                return _changes;
            }
            set
            {
                _changes = value;

                NotifyPropertyChanged("Changes");
            }
        }
    }

    
}
