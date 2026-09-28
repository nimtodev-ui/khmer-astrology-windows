namespace KhmerAstrology.Domain.Models;

/// <summary>
/// A single daily row in the Automatic Calendar table (matching sheet 30 rows 6+).
/// </summary>
public sealed record AutomaticCalendarDayRow(
    int DayNumber,
    string Weekday,
    int Day,
    string MonthName,
    int CeYear,
    int BuddhistYear,
    int MahaSakaraj,
    int ChulaSakaraj,
    int KromSakaraj,
    string LunarDay,
    string TithiName,
    string LunarMonth,
    string AnimalYear,
    int SesaKalaYoga,
    int Yuga,
    string SamvatsaraName,
    string Meaning,
    int LunarDayNumber = 0,
    int LunarMonthLength = 0)
{
    /// <summary>Buddhist holy day (ថ្ងៃសីល) from the day walk's lunar day and month length.</summary>
    public bool IsHolyDay => KhmerLunarCalendarRules.IsHolyDay(LunarDayNumber, LunarMonthLength);

    /// <summary>Moon phase marked on the standard calendar for this day.</summary>
    public KhmerMoonPhase MoonPhase => KhmerLunarCalendarRules.GetMoonPhase(LunarDayNumber, LunarMonthLength);

    /// <summary>Buddhist observance fixed by this lunar date, if any.</summary>
    public KhmerLunarObservance? Observance =>
        KhmerLunarCalendarRules.GetObservance(LunarMonth, LunarDayNumber, LunarMonthLength);
}

/// <summary>
/// Full month calculation result for the Automatic Calendar tab.
/// </summary>
public sealed record AutomaticCalendarMonthResult(
    int AstronomicalYear,
    int CeOrBceYear,
    int MonthIndex,
    string MonthName,
    int KromSakarajAuto,
    IReadOnlyList<AutomaticCalendarDayRow> Days);

/// <summary>
/// Result of searching a specific date in the Khmer calendar system (Sheet 31: ស្វែងរក ថ្ងៃខែឆ្នាំ).
/// </summary>
public sealed record KhmerDateSearchResult(
    int Day,
    int Month,
    string MonthName,
    int CeOrBceYear,
    int AstronomicalYear,
    int KromSakarajAuto,
    string SolarFullDate,
    string SolarWeekday,
    int SolarDay,
    string SolarMonth,
    string SolarYear,
    string LunarWeekday,
    string LunarDay,
    string TithiName,
    string LunarMonth,
    string AnimalYear,
    int BuddhistYear,
    int MahaSakaraj,
    int ChulaSakaraj,
    int KromSakaraj,
    int SesaKalaYoga,
    bool IsHolyDay = false);
