namespace KhmerAstrology.Domain.Models;

/// <summary>
/// Lunar state of one Gregorian day from the sheet 30 (ប្រតិទិនស្វ័យប្រវត្តិ) day walk,
/// columns R:V: Khmer year (R), lunar-year type (S, 2 = 13-month year), lunar
/// month index (T, 1-based), lunar day (U, 1..30) and month length (V, 29/30).
/// </summary>
public sealed record KhmerLunarState(
    int KhmerYear,
    int LunarYearType,
    int LunarMonthIndex,
    int LunarDay,
    int MonthLength)
{
    public bool IsWaxing => LunarDay <= 15;

    public int DayInPhase => IsWaxing ? LunarDay : LunarDay - 15;

    /// <summary>Workbook K6: day 15 is the full moon; days 29 and 30 are អមាវសី.</summary>
    public bool IsFullMoon => LunarDay == 15;

    public bool IsNewMoon => LunarDay is 29 or 30;
}
