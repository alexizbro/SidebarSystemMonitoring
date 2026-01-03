using System;
using System.IO;
using System.Reflection;

namespace SidebarSystemMonitoring.Utilities;

public static class Paths
{
    public static string AssemblyName => Assembly.GetExecutingAssembly().GetName().Name;
    public static string ChangeLog { get; }
    public static string CurrentDirectory => AppDomain.CurrentDomain.BaseDirectory;
    public static string ExeName { get; }
    public static string LocalApp { get; }
    public static string SettingsFile { get; }
    public static string TaskBar { get; }
    
    private const string SETTINGS = "settings.json";
    private const string CHANGELOG = "ChangeLog.json";

    static Paths()
    {
        ChangeLog = Path.Combine(CurrentDirectory, CHANGELOG);
        ExeName = $"{AssemblyName}.exe";
        LocalApp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AssemblyName);
        
        string currentDirPath = Path.Combine(Environment.CurrentDirectory, SETTINGS);
        string localAppPath = Path.Combine(LocalApp, SETTINGS);
        
        SettingsFile = File.Exists(currentDirPath) ? currentDirPath : localAppPath;
        
        TaskBar = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar");
    }

    public static string Install(Version version)
    {
        return Path.Combine(LocalApp, $"app-{version.ToString(3)}");
    }

    public static string GetExe(Version version)
    {
        return Path.Combine(Install(version), ExeName);
    }
}