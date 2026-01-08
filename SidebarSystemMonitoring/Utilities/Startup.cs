using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Microsoft.Win32.TaskScheduler;
using SidebarSystemMonitoring.Framework;

namespace SidebarSystemMonitoring.Utilities;

public static class Startup
{
    public static bool StartupTaskExists()
    {
        using (TaskService taskService = new TaskService())
        {
            Task task = taskService.FindTask(Constants.Generic.TASKNAME);

            if (task == null)
                return false;

            ExecAction action = task.Definition.Actions.OfType<ExecAction>().FirstOrDefault();

            string currentExe = Process.GetCurrentProcess().MainModule.FileName;

            // Check if it points to the correct exe (not a DLL or SYS)
            if (action == null || !string.Equals(action.Path, currentExe, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }
    }

    public static void EnableStartupTask(string exePath = null)
    {
        try
        {
            using (TaskService taskService = new TaskService())
            {
                // Remove any legacy SidebarSystemMonitoring tasks that point to wrong files
                CleanLegacyTasks(taskService);

                TaskDefinition def = taskService.NewTask();
                def.Triggers.Add(new LogonTrigger { Enabled = true });

                string targetExe = exePath ?? Process.GetCurrentProcess().MainModule.FileName;
                def.Actions.Add(new ExecAction(targetExe));

                def.Principal.RunLevel = TaskRunLevel.Highest;
                def.Settings.DisallowStartIfOnBatteries = false;
                def.Settings.StopIfGoingOnBatteries = false;
                def.Settings.ExecutionTimeLimit = TimeSpan.Zero;

                taskService.RootFolder.RegisterTaskDefinition(Constants.Generic.TASKNAME, def);
            }
        }
        catch (Exception e)
        {
            using (EventLog log = new EventLog("Application"))
            {
                log.Source = Resources.AppName;
                log.WriteEntry(e.ToString(), EventLogEntryType.Error, 100, 1);
            }
        }
    }

    public static void DisableStartupTask()
    {
        using (TaskService taskService = new TaskService())
        {
            taskService.RootFolder.DeleteTask(Constants.Generic.TASKNAME, false);
        }
    }

    /// <summary>
    /// Deletes legacy SidebarSystemMonitoring tasks that point to DLL or SYS files.
    /// </summary>
    private static void CleanLegacyTasks(TaskService taskService)
    {
        string[] legacyPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SidebarSystemMonitoring", "SidebarSystemMonitoring.dll"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SidebarSystemMonitoring", "SidebarSystemMonitoring.sys"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SidebarSystemMonitoring", "SidebarSystemMonitoring.exe")
        };

        foreach (Task t in taskService.RootFolder.AllTasks)
        {
            if (t.Name.Equals(Constants.Generic.TASKNAME, StringComparison.OrdinalIgnoreCase))
            {
                var action = t.Definition.Actions.OfType<ExecAction>().FirstOrDefault();
                if (action != null && legacyPaths.Any(lp => string.Equals(lp, action.Path, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        taskService.RootFolder.DeleteTask(t.Name, false);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to delete legacy task {t.Name}: {ex.Message}");
                    }
                }
            }
        }
    }
}