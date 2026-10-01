using System.Globalization;

namespace MyFundex.Risk;

public static class TradingWindow
{
    public static string? Encode(string? start, string? end)
    {
        if (start is null && end is null) return null;
        if (!TimeOnly.TryParseExact(start, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var opening)
            || !TimeOnly.TryParseExact(end, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var closing)
            || opening >= closing)
            throw new ArgumentException("Provide both start and end times in HH:mm format. End time must be later on the same day.");
        return $"{opening:HH:mm}|{closing:HH:mm}";
    }

    public static bool Allows(string? window, string side, DateTimeOffset now)
    {
        var parts = window?.Split('|');
        if (parts?.Length != 2 || Encode(parts[0], parts[1]) != window)
            throw new ArgumentException("Trading window is invalid.");
        if (side == "SELL") return true; // Never prevent an exit because of the entry window.
        var indiaTime = TimeOnly.FromDateTime(now.ToOffset(TimeSpan.FromHours(5.5)).DateTime);
        var start = TimeOnly.ParseExact(parts[0], "HH:mm", CultureInfo.InvariantCulture);
        var end = TimeOnly.ParseExact(parts[1], "HH:mm", CultureInfo.InvariantCulture);
        return indiaTime >= start && indiaTime < end;
    }
}
