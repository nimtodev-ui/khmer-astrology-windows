namespace KhmerAstrology.Domain.Models;

/// <summary>
/// One encoded month entry extracted from the workbook's automatic calendar
/// table. The encoded fields preserve the workbook's lookup-table boundary;
/// date-level presentation is calculated by <c>KhmerCalendarCalculator</c>.
/// </summary>
public sealed record KhmerCalendarMonthReference(
    int AstronomicalYear,
    int GregorianMonth,
    int KhmerYear,
    int LunarYearType,
    int IntercalaryFlag,
    int LunarMonthIndex,
    int TithiOffset);
