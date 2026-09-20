using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Calculation.Suriyayatra.Models;
using KhmerAstrology.Domain.Constants;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Suriyayatra;

/// <summary>
/// Ports the traditional Sun chain from ព្រះអាទិត្យ!B10:B38.
/// This is kept separate from the modern longitude chain in ក្បួនគណនា.
/// </summary>
public sealed class SolarCalculator : ISolarCalculator
{
    private static readonly int[] ShadowBases = [0, 35, 67, 94, 116, 129, 134];
    private static readonly int[] ShadowDifferences = [35, 32, 27, 22, 13, 5, 0];

    public SolarCalculationResult Calculate(KhmerCalendarResult calendar)
    {
        ArgumentNullException.ThrowIfNull(calendar);

        var meanRemainder = Mod(
            calendar.AharganaDay * 800D + calendar.Kammaja - 373D,
            292_207D);
        var meanLongitude = (int)Math.Floor(meanRemainder * AstrologyConstants.FullCircleArcMinutes / 292_207D);
        const int exaltationLongitude = 4_800;

        var meanAnomaly = Mod(meanLongitude - exaltationLongitude, AstrologyConstants.FullCircleArcMinutes);
        var anomalySign = (int)Math.Floor(meanAnomaly / AstrologyConstants.SignArcMinutes);
        var anomalyDegree = (int)Math.Floor(Mod(meanAnomaly, AstrologyConstants.SignArcMinutes) / 60D);
        var anomalyMinute = (int)Math.Floor(Mod(meanAnomaly, 60D));
        var segment = Math.Min(6, (int)Math.Floor((anomalyDegree * 60D + anomalyMinute) / 600D));
        var segmentRemainder = anomalyDegree * 60D + anomalyMinute - segment * 600D;
        var correction =
            (int)Math.Floor(segmentRemainder * ShadowDifferences[segment] / 600D)
            + (Mod(segmentRemainder * ShadowDifferences[segment], 600D) > 300D ? 1 : 0);
        var correctionWithSign = (anomalySign >= 6 ? 1 : -1) * (ShadowBases[segment] + correction);
        var longitude = Mod(meanLongitude + correctionWithSign, AstrologyConstants.FullCircleArcMinutes);

        return new SolarCalculationResult(meanRemainder, meanLongitude, longitude);
    }

    private static double Mod(double value, double divisor) => ((value % divisor) + divisor) % divisor;
}
