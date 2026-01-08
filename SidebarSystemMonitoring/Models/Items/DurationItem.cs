namespace SidebarSystemMonitoring.Models.Items;

public class DurationItem
{
    public DurationItem(int seconds, string text)
    {
        Seconds = seconds;
        Text = text;
    }

    public int Seconds { get; set; }

    public string Text { get; set; }
}
