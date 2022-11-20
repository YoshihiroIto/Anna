namespace Anna.Foundation;

public static class DateTimeHelper
{
    public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
    {
        return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
    }
}