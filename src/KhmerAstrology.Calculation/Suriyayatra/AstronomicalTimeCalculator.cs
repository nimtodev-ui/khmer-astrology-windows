using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Suriyayatra;

internal static class AstronomicalTimeCalculator
{
    /// <summary>
    /// Calculates workbook <c>ក្បួនគណនា!Q56</c> from the local civil date/time
    /// and the configured timezone offset, using the same Gregorian-to-Julian
    /// conversion used by the workbook's <c>Q55</c> formula.
    /// </summary>
    public static double CalculateUtcJulianDay(BirthInput input)
    {
        var localDateTime = input.BirthDate.ToDateTime(input.BirthTime);
        var localJulianDay = CalculateLocalJulianDay(localDateTime);
        return localJulianDay - ResolveUtcOffset(input, localDateTime).TotalHours / 24D;
    }

    public static double CalculateLocalJulianDay(BirthInput input)
    {
        return CalculateLocalJulianDay(input.BirthDate.ToDateTime(input.BirthTime));
    }

    private static double CalculateLocalJulianDay(DateTime localDateTime)
    {
        var year = localDateTime.Year;
        var month = localDateTime.Month;
        var adjustedYear = year - (month <= 2 ? 1 : 0);
        var adjustedMonth = month + (month <= 2 ? 12 : 0);
        var localJulianDay =
            Math.Floor(365.25D * (adjustedYear + 4_716D))
            + Math.Floor(30.6001D * (adjustedMonth + 1D))
            + localDateTime.Day
            + (2D - Math.Floor(adjustedYear / 100D) + Math.Floor(Math.Floor(adjustedYear / 100D) / 4D))
            - 1_524.5D
            + localDateTime.TimeOfDay.TotalDays;

        return localJulianDay;
    }

    public static TimeSpan ResolveUtcOffset(DateTime localDateTime, string timeZoneId)
    {
        try
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return zone.GetUtcOffset(localDateTime);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeSpan.Zero;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeSpan.Zero;
        }
    }

    private static TimeSpan ResolveUtcOffset(BirthInput input, DateTime localDateTime)
    {
        return input.UtcOffsetOverrideHours is double overrideHours
            ? TimeSpan.FromHours(overrideHours)
            : ResolveUtcOffset(localDateTime, input.TimeZoneId);
    }
}
