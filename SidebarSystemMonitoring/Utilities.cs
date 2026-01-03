using System;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Win32.TaskScheduler;
using SidebarSystemMonitoring.Framework;
using System.Diagnostics;

namespace SidebarSystemMonitoring.Utilities
{
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

    public static class Culture
    {
        public const string DEFAULT = "Default";

        public static void SetDefault()
        {
            Default = Thread.CurrentThread.CurrentUICulture;
        }

        public static void SetCurrent(bool init)
        {
            Resources.Culture = CultureInfo;

            Thread.CurrentThread.CurrentCulture = CultureInfo;
            Thread.CurrentThread.CurrentUICulture = CultureInfo;

            if (init)
            {
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));
            }
        }

        public static CultureItem[] GetAll()
        {
            return new CultureItem[1] { new CultureItem() { Value = DEFAULT, Text = Resources.SettingsLanguageDefault } }.Concat(CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(c => Languages.Contains(c.TwoLetterISOLanguageName)).OrderBy(c => c.DisplayName).Select(c => new CultureItem() { Value = c.Name, Text = c.DisplayName })).ToArray();
        }

        public static string[] Languages
        {
            get
            {
                return new string[11] { "en", "da", "de", "fr", "ja", "nl", "zh", "it", "ru", "fi", "es" };
            }
        }

        public static CultureInfo Default { get; private set; }

        public static CultureInfo CultureInfo
        {
            get
            {
                string culture = Framework.Settings.Instance.Culture;
                return string.Equals(culture, DEFAULT, StringComparison.Ordinal)
                    ? Default
                    : new CultureInfo(culture);
            }
        }
    }

    public class CultureItem
    {
        public string Value { get; set; }

        public string Text { get; set; }
    }
}
