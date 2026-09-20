using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Calculation.Suriyayatra.Models;
using KhmerAstrology.Domain.Constants;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Suriyayatra;

/// <summary>
/// Ports the traditional Moon chain from ព្រះចន្ទ!B12:B46.
/// </summary>
public sealed class LunarCalculator : ILunarCalculator
{
    private static readonly int[] ShadowBases = [0, 77, 148, 209, 256, 286, 296];
    private static readonly int[] ShadowDifferences = [77, 71, 61, 47, 30, 10, 0];

    public LunarCalculationResult Calculate(KhmerCalendarResult calendar, SolarCalculationResult sun)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        ArgumentNullException.ThrowIfNull(sun);

        var avamanaRemainder = calendar.Avamana + Math.Floor(calendar.Avamana / 25D);
        var boriTithiArcMinutes = calendar.BoriTithi * 720D;
        var preMeanMoon = Mod(avamanaRemainder + boriTithiArcMinutes - 40D, AstrologyConstants.FullCircleArcMinutes);
        var meanMoon = Mod(preMeanMoon + sun.MeanLongitudeArcMinutes, AstrologyConstants.FullCircleArcMinutes);

        var meanExaltation =
            Math.Floor(calendar.Uccabal * 3D / 808D) * AstrologyConstants.SignArcMinutes
            + Math.Floor(Mod(calendar.Uccabal * 3D, 808D) * 30D / 808D) * 60D
            + Math.Floor(Mod(Mod(calendar.Uccabal * 3D, 808D) * 30D, 808D) * 60D / 808D)
            + 2D;
        var specialExaltation = Mod(meanMoon - meanExaltation, AstrologyConstants.FullCircleArcMinutes);
        var specialSign = (int)Math.Floor(specialExaltation / AstrologyConstants.SignArcMinutes);
        var openingDistance = specialSign <= 2
            ? specialExaltation
            : specialSign <= 5
                ? 10_800D - specialExaltation
                : specialSign <= 8
                    ? specialExaltation - 10_800D
                    : 21_600D - specialExaltation;
        var segment = Math.Min(6, (int)Math.Floor(openingDistance / 900D));
        var segmentRemainder = openingDistance - segment * 900D;
        var correction =
            (int)Math.Floor(segmentRemainder * ShadowDifferences[segment] / 900D)
            + (Mod(segmentRemainder * ShadowDifferences[segment], 900D) > 450D ? 1 : 0);
        var signedCorrection = (specialSign >= 6 ? 1 : -1) * (ShadowBases[segment] + correction);
        var longitude = Mod(meanMoon + signedCorrection, AstrologyConstants.FullCircleArcMinutes);

        return new LunarCalculationResult((int)Math.Floor(meanMoon), longitude);
    }

    private static double Mod(double value, double divisor) => ((value % divisor) + divisor) % divisor;
}
