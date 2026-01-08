using System;
using System.Windows;
using SidebarSystemMonitoring.Models;
using SidebarSystemMonitoring.Windows;
using SidebarSystemMonitoring.Style;
using SidebarSystemMonitoring.Models;

namespace SidebarSystemMonitoring
{
    /// <summary>
    /// Interaction logic for ChangeLog.xaml
    /// </summary>
    public partial class ChangeLog : FlatWindow
    {
        public ChangeLog(Version version)
        {
            InitializeComponent();

            DataContext = Model = new ChangeLogModel(version);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public ChangeLogModel Model { get; private set; }
    }
}
