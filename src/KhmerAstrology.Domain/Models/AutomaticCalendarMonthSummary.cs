namespace KhmerAstrology.Domain.Models;

/// <summary>
/// First and last value of a calendar field within one month. The two differ
/// when the Khmer New Year falls inside the month (sheet 30 changes ព.ស. and the
/// era/year columns from day-of-year 106, i.e. mid-April).
/// </summary>
public sealed record MonthValueSpan<T>(T First, T Last)
{
    public bool Changes => !EqualityComparer<T>.Default.Equals(First, Last);
}

/// <summary>
/// Month-level view of an <see cref="AutomaticCalendarMonthResult"/>: the columns
/// that repeat on every row of sheet 30, plus the full- and new-moon days named
/// by the sheet's tithi column (K6: ពេញបូណ៌មី / អមាវសី).
/// </summary>
public sealed record AutomaticCalendarMonthSummary(
    MonthValueSpan<int> BuddhistYear,
    MonthValueSpan<int> MahaSakaraj,
    MonthValueSpan<int> ChulaSakaraj,
    MonthValueSpan<int> KromSakaraj,
    MonthValueSpan<string> AnimalYear,
    MonthValueSpan<int> SesaKalaYoga,
    MonthValueSpan<int> Yuga,
    MonthValueSpan<string> SamvatsaraName,
    MonthValueSpan<string> SamvatsaraMeaning,
    IReadOnlyList<int> FullMoonDays,
    IReadOnlyList<int> NewMoonDays,
    int? YearChangeDay)
{
    public const string FullMoonTithi = "ពេញបូណ៌មី";
    public const string NewMoonTithi = "អមាវសី";

    public static AutomaticCalendarMonthSummary From(AutomaticCalendarMonthResult month)
    {
        ArgumentNullException.ThrowIfNull(month);
        if (month.Days.Count == 0)
        {
            throw new ArgumentException("The month has no days.", nameof(month));
        }

        MonthValueSpan<T> Span<T>(Func<AutomaticCalendarDayRow, T> selector) =>
            new(selector(month.Days[0]), selector(month.Days[^1]));

        return new AutomaticCalendarMonthSummary(
            Span(day => day.BuddhistYear),
            Span(day => day.MahaSakaraj),
            Span(day => day.ChulaSakaraj),
            Span(day => day.KromSakaraj),
            Span(day => day.AnimalYear),
            Span(day => day.SesaKalaYoga),
            Span(day => day.Yuga),
            Span(day => day.SamvatsaraName),
            Span(day => day.Meaning),
            month.Days.Where(day => day.TithiName == FullMoonTithi).Select(day => day.Day).ToArray(),
            month.Days.Where(day => day.TithiName == NewMoonTithi).Select(day => day.Day).ToArray(),
            month.Days.FirstOrDefault(day => day.BuddhistYear != month.Days[0].BuddhistYear)?.Day);
    }

    /// <summary>
    /// Moves a CE/BCE year + Gregorian month by <paramref name="months"/>, skipping
    /// the non-existent year 0 (1 BCE is followed by 1 CE).
    /// </summary>
    public static (int CeOrBceYear, int Month) StepMonth(int ceOrBceYear, int month, int months)
    {
        if (ceOrBceYear == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ceOrBceYear), "Year 0 does not exist.");
        }

        var astronomicalYear = ceOrBceYear < 0 ? ceOrBceYear + 1 : ceOrBceYear;
        var totalMonths = astronomicalYear * 12 + (month - 1) + months;
        var newAstronomicalYear = (int)Math.Floor(totalMonths / 12D);
        var newMonth = totalMonths - newAstronomicalYear * 12 + 1;
        var newYear = newAstronomicalYear <= 0 ? newAstronomicalYear - 1 : newAstronomicalYear;
        return (newYear, newMonth);
    }
}
