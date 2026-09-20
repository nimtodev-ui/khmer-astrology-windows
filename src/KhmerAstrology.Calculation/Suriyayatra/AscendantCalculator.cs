using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Calculation.Suriyayatra.Models;

namespace KhmerAstrology.Calculation.Suriyayatra;

/// <summary>
/// Ports the astronomical Ascendant chain from <c>ក្បួនគណនា!Q41:Q52</c>.
/// </summary>
public sealed class AscendantCalculator : IAscendantCalculator
{
    private const double J2000JulianDay = 2_451_545D;
    private const double ExcelJulianDayOffset = 2_415_018.5D;
    private const double DaysPerJulianCentury = 36_525D;
    private const double FullCircleDegrees = 360D;
    private const double ArcMinutesPerDegree = 60D;

    public double CalculateLongitudeArcMinutes(BirthInput input)
    {
        return Calculate(input).SiderealAscendantArcMinutes;
    }

    /// <summary>
    /// Calculates the Ascendant while preserving the workbook's named intermediate steps.
    /// </summary>
    /// <remarks>
    /// Source formulas:
    /// <list type="table">
    /// <item><term>Q41</term><description>Q56 - 2415018.5</description></item>
    /// <item><term>Q42</term><description>Q56</description></item>
    /// <item><term>Q43</term><description>(Q42 - 2451545) / 36525</description></item>
    /// <item><term>Q44</term><description>MOD(280.46061837 + 360.98564736629 × (Q42 - 2451545) + 0.000387933 × Q43² - Q43³ / 38710000, 360)</description></item>
    /// <item><term>Q46</term><description>MOD(Q44 + Q45, 360)</description></item>
    /// <item><term>Q48</term><description>Mean-obliquity polynomial from Q43</description></item>
    /// <item><term>Q49</term><description>Workbook quadrant-aware tropical Ascendant formula</description></item>
    /// <item><term>Q50</term><description>Lahiri ayanāṃśa polynomial from Q43</description></item>
    /// <item><term>Q51</term><description>MOD(Q49 - Q50, 360)</description></item>
    /// <item><term>Q52</term><description>Q51 × 60</description></item>
    /// </list>
    /// </remarks>
    public AscendantCalculationResult Calculate(BirthInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ValidateCoordinates(input);

        var utcJulianDay = AstronomicalTimeCalculator.CalculateUtcJulianDay(input);
        var utcExcelSerial = utcJulianDay - ExcelJulianDayOffset;
        var julianCenturies = (utcJulianDay - J2000JulianDay) / DaysPerJulianCentury;
        var meanSiderealTime = Mod(
            280.46061837D
            + 360.98564736629D * (utcJulianDay - J2000JulianDay)
            + 0.000387933D * julianCenturies * julianCenturies
            - Math.Pow(julianCenturies, 3) / 38_710_000D,
            FullCircleDegrees);
        var localSiderealTime = Mod(meanSiderealTime + input.Longitude, FullCircleDegrees);
        var obliquity =
            23D
            + 26D / 60D
            + 21.448D / 3_600D
            - (46.815D * julianCenturies
                + 0.00059D * julianCenturies * julianCenturies
                - 0.001813D * Math.Pow(julianCenturies, 3)) / 3_600D;
        var latitudeRadians = DegreesToRadians(input.Latitude);
        var localSiderealTimeRadians = DegreesToRadians(localSiderealTime);
        var obliquityRadians = DegreesToRadians(obliquity);
        var numerator = -Math.Cos(localSiderealTimeRadians);
        var denominator =
            Math.Sin(localSiderealTimeRadians) * Math.Cos(obliquityRadians)
            + Math.Tan(latitudeRadians) * Math.Sin(obliquityRadians);
        var tropicalAscendant = Mod(
            RadiansToDegrees(Math.Atan(numerator / denominator))
            + (denominator < 0D ? 180D : numerator < 0D ? 360D : 0D)
            + 180D,
            FullCircleDegrees);
        var precession =
            23.8570924D
            + 1.39688796D * julianCenturies
            + 0.000307090434D * julianCenturies * julianCenturies
            + 0.00000000445316645D * Math.Pow(julianCenturies, 3);
        var siderealLongitude = Mod(tropicalAscendant - precession, FullCircleDegrees);

        return new AscendantCalculationResult
        {
            UtcExcelSerial = utcExcelSerial,
            UtcJulianDay = utcJulianDay,
            JulianCenturies = julianCenturies,
            MeanSiderealTimeDegrees = meanSiderealTime,
            LongitudeDegrees = input.Longitude,
            LocalSiderealTimeDegrees = localSiderealTime,
            LatitudeDegrees = input.Latitude,
            MeanObliquityDegrees = obliquity,
            TropicalAscendantDegrees = tropicalAscendant,
            LahiriAyanamsaDegrees = precession,
            SiderealAscendantDegrees = siderealLongitude,
            SiderealAscendantArcMinutes = siderealLongitude * ArcMinutesPerDegree,
        };
    }

    private static void ValidateCoordinates(BirthInput input)
    {
        if (!double.IsFinite(input.Latitude) || input.Latitude is < -90D or > 90D)
        {
            throw new ArgumentOutOfRangeException(nameof(input.Latitude), input.Latitude, "Latitude must be between -90 and 90 degrees.");
        }

        if (!double.IsFinite(input.Longitude) || input.Longitude is < -180D or > 180D)
        {
            throw new ArgumentOutOfRangeException(nameof(input.Longitude), input.Longitude, "Longitude must be between -180 and 180 degrees.");
        }
    }

    private static double DegreesToRadians(double value) => value * Math.PI / 180D;

    private static double RadiansToDegrees(double value) => value * 180D / Math.PI;

    private static double Mod(double value, double divisor) => ((value % divisor) + divisor) % divisor;
}
