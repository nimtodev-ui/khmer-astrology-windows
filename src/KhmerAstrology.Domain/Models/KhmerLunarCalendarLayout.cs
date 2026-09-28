namespace KhmerAstrology.Domain.Models;

/// <summary>
/// Date ranges for the standard Khmer wall-calendar views (week, month, year).
/// Weeks start on Sunday (អាទិត្យ), matching the sheet 30 weekday order.
/// </summary>
public static class KhmerLunarCalendarLayout
{
    /// <summary>Weeks shown by a month grid; six rows fit every month.</summary>
    public const int MonthGridWeeks = 6;

    /// <summary>Sunday on or before <paramref name="date"/>.</summary>
    public static DateTime WeekStart(DateTime date)
    {
        var day = date.Date;
        return SafeAddDays(day, -(int)day.DayOfWeek);
    }

    /// <summary>First cell of the 6 × 7 month grid: the Sunday on or before the 1st.</summary>
    public static DateTime MonthGridStart(int year, int month) => WeekStart(new DateTime(year, month, 1));

    /// <summary>Number of week rows the month actually uses (4..6).</summary>
    public static int MonthWeekRows(int year, int month)
    {
        var first = new DateTime(year, month, 1);
        var cells = (int)first.DayOfWeek + DateTime.DaysInMonth(year, month);
        return (cells + 6) / 7;
    }

    /// <summary>Moves a date by whole weeks, months or years, clamped to the DateTime range.</summary>
    public static DateTime Step(DateTime date, KhmerLunarCalendarView view, int delta)
    {
        try
        {
            return view switch
            {
                KhmerLunarCalendarView.Week => date.AddDays(7 * delta),
                KhmerLunarCalendarView.Month => date.AddMonths(delta),
                KhmerLunarCalendarView.Year => date.AddYears(delta),
                _ => date,
            };
        }
        catch (ArgumentOutOfRangeException)
        {
            return delta < 0 ? DateTime.MinValue.Date : DateTime.MaxValue.Date;
        }
    }

    private static DateTime SafeAddDays(DateTime date, int days)
    {
        var ticks = date.Ticks + days * TimeSpan.TicksPerDay;
        return ticks < DateTime.MinValue.Ticks ? DateTime.MinValue.Date : new DateTime(ticks);
    }
}

public enum KhmerLunarCalendarView
{
    Week,
    Month,
    Year,
}
