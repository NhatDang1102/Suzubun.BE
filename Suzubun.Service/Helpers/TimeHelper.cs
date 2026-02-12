namespace Suzubun.Service.Helpers;

public static class TimeHelper
{
    public static DateTime GetVietnamTime()
    {
        var utcNow = DateTime.UtcNow;
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(utcNow, vietnamTimeZone);
    }

    public static DateTime ToVietnamTime(this DateTime utcDate)
    {
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(utcDate, vietnamTimeZone);
    }
}
