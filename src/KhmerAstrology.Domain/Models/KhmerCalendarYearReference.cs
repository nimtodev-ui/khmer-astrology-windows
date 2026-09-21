namespace KhmerAstrology.Domain.Models;

/// <summary>
/// Authoritative year-level reference record for the automatic calendar (sheet 30).
/// Extracted from the 5,103-year table covering astronomical years -643 to 4459.
/// </summary>
public sealed record KhmerCalendarYearReference(
    int AstronomicalYear,
    int KhmerYear,
    int LunarYearType,
    int IntercalaryFlag1,
    int IntercalaryFlag2,
    int StartMonthIndex,
    int StartTithiOffset);
