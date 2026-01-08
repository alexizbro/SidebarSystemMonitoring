using SidebarSystemMonitoring.Windows.Enums;
using SidebarSystemMonitoring.Windows.Structs;

namespace SidebarSystemMonitoring.Windows;

public class WorkArea
{
    public double Left { get; set; }

    public double Top { get; set; }

    public double Right { get; set; }

    public double Bottom { get; set; }

    public double Width
    {
        get
        {
            return Right - Left;
        }
    }

    public double Height
    {
        get
        {
            return Bottom - Top;
        }
    }

    public void Scale(double x, double y)
    {
        Left *= x;
        Top *= y;
        Right *= x;
        Bottom *= y;
    }

    public void Offset(double x, double y)
    {
        Left += x;
        Top += y;
        Right += x;
        Bottom += y;
    }

    public void SetWidth(DockEdge edge, double width)
    {
        switch (edge)
        {
            case DockEdge.Left:
                Right = Left + width;
                break;

            case DockEdge.Right:
                Left = Right - width;
                break;
        }
    }

    public static WorkArea FromRECT(RECT rect)
    {
        return new WorkArea()
        {
            Left = rect.Left,
            Top = rect.Top,
            Right = rect.Right,
            Bottom = rect.Bottom
        };
    }
}