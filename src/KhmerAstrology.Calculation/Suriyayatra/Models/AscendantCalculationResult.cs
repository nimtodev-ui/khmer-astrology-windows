namespace KhmerAstrology.Calculation.Suriyayatra.Models;

/// <summary>
/// Intermediate values produced by the workbook's astronomical Ascendant chain.
/// The values correspond to <c>ក្បួនគណនា!Q41:Q52</c>.
/// </summary>
public sealed record AscendantCalculationResult
{
    /// <summary>Workbook Q41: UTC Excel serial day.</summary>
    public double UtcExcelSerial { get; init; }

    /// <summary>Workbook Q42: UTC Julian Day.</summary>
    public double UtcJulianDay { get; init; }

    /// <summary>Workbook Q43: Julian centuries from J2000.0.</summary>
    public double JulianCenturies { get; init; }

    /// <summary>Workbook Q44: Greenwich mean sidereal time in degrees.</summary>
    public double MeanSiderealTimeDegrees { get; init; }

    /// <summary>Workbook Q45: active location longitude in degrees.</summary>
    public double LongitudeDegrees { get; init; }

    /// <summary>Workbook Q46: local sidereal time in degrees.</summary>
    public double LocalSiderealTimeDegrees { get; init; }

    /// <summary>Workbook Q47: active location latitude in degrees.</summary>
    public double LatitudeDegrees { get; init; }

    /// <summary>Workbook Q48: mean obliquity of the ecliptic in degrees.</summary>
    public double MeanObliquityDegrees { get; init; }

    /// <summary>Workbook Q49: tropical Ascendant in degrees.</summary>
    public double TropicalAscendantDegrees { get; init; }

    /// <summary>Workbook Q50: Lahiri ayanāṃśa in degrees.</summary>
    public double LahiriAyanamsaDegrees { get; init; }

    /// <summary>Workbook Q51: sidereal Ascendant in degrees.</summary>
    public double SiderealAscendantDegrees { get; init; }

    /// <summary>Workbook Q52: sidereal Ascendant in arcminutes.</summary>
    public double SiderealAscendantArcMinutes { get; init; }
}
