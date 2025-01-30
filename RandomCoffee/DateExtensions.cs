namespace RandomCoffee;

public static class DateExtensions
{
    public static DateTime GetNext(this DateTime fromDate, DayOfWeek dayOfWeek)
    {
        // Calculate the days until the next occurence
        var daysUntilDay = ((int) dayOfWeek - (int) fromDate.DayOfWeek + 7) % 7;

        // If today is that day of week, we want the next occurence, not today
        if (daysUntilDay == 0) daysUntilDay = 7;

        return fromDate.AddDays(daysUntilDay);
    }
}