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
    string Meaning);

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
    int SesaKalaYoga);
