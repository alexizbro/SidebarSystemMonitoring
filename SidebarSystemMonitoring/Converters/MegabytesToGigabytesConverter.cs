namespace SidebarSystemMonitoring.Converters;

public static class MegabytesToGigabytesConverter
{
    public static float ConvertMegabytesToGigabytes(float value)
    {
        return value / 1024;
    }

    public static float ConvertGigabytesToMegabytes(float value)
    {
        return value * 1024;
    }
}