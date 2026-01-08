using System;
using System.Windows.Input;

namespace SidebarSystemMonitoring.Commands;

public class ActivateCommand : ICommand
{
    public void Execute(object parameter)
    {
        Sidebar sidebar = App.Current.Sidebar;

        if (sidebar == null)
        {
            return;
        }
            
        sidebar.Activate();
    }

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;
}