using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Calculation.Suriyayatra.Models;
using KhmerAstrology.Domain.Constants;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Suriyayatra;

/// <summary>
/// Ports the four-motion traditional chains from workbook sheets 011, 012, and 013.
/// The table coefficients are reference data; the staged arithmetic remains explicit here.
/// </summary>
public sealed class TraditionalOuterPointCalculator : ITraditionalOuterPointCalculator
{
    private readonly IReadOnlyList<TraditionalOuterPointReference> _references;

    public TraditionalOuterPointCalculator(ITraditionalOuterPointReferenceDataSource referenceDataSource)
    {
        _references = referenceDataSource.Load();
    }

    public IReadOnlyList<TraditionalOuterPointPosition> Calculate(
        KhmerCalendarResult calendar,
        SolarCalculationResult solar)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        ArgumentNullException.ThrowIfNull(solar);

        return _references.Select(reference =>
        {
            var body = Enum.Parse<CelestialBody>(reference.Body, ignoreCase: false);
            return new TraditionalOuterPointPosition(body, CalculateLongitude(reference, calendar, solar));
        }).ToArray();
    }

    private static double CalculateLongitude(
        TraditionalOuterPointReference reference,
        KhmerCalendarResult calendar,
        SolarCalculationResult solar)
    {
        var totalDays = calendar.AharganaDay + reference.MeanOffset;
        var fullCycles = Math.Floor((double)totalDays / reference.MeanPeriod);
        var remainder = totalDays - fullCycles * reference.MeanPeriod;
        var meanValue = remainder * AstrologyConstants.FullCircleArcMinutes / reference.MeanPeriod;
        var meanLongitude = reference.RoundMeanLongitude
            ? Math.Round(meanValue, 0, MidpointRounding.AwayFromZero)
            : Math.Floor(meanValue);

        var firstAngle = Mod(solar.MeanLongitudeArcMinutes - meanLongitude, AstrologyConstants.FullCircleArcMinutes);
        var firstSign = SignIndex(firstAngle);
        var firstBhujja = CalculateBhujja(firstAngle, firstSign, reference, stage: 1);
        var firstShadow = CalculateShadow(firstBhujja, reference.FirstTableBase, reference.FirstTableDifference);
        var firstHalfShadow = Math.Round(firstShadow / 2D, 0, MidpointRounding.AwayFromZero);
        var firstLongitude = firstSign <= 5
            ? Mod(reference.ExaltationArcMinutes + firstHalfShadow, AstrologyConstants.FullCircleArcMinutes)
            : Mod(reference.ExaltationArcMinutes - firstHalfShadow, AstrologyConstants.FullCircleArcMinutes);

        var secondAngle = Mod(meanLongitude - firstLongitude, AstrologyConstants.FullCircleArcMinutes);
        var secondSign = SignIndex(secondAngle);
        var secondBhujja = CalculateBhujja(secondAngle, secondSign, reference, stage: 2);
        var secondShadow = CalculateShadow(secondBhujja, reference.SecondTableBase, reference.SecondTableDifference);
        var secondHalfShadow = Math.Round(secondShadow / 2D, 0, MidpointRounding.AwayFromZero);
        var secondLongitude = secondSign <= 5
            ? Mod(firstLongitude - secondHalfShadow, AstrologyConstants.FullCircleArcMinutes)
            : Mod(firstLongitude + secondHalfShadow, AstrologyConstants.FullCircleArcMinutes);

        var thirdAngle = Mod(meanLongitude - secondLongitude, AstrologyConstants.FullCircleArcMinutes);
        var thirdSign = SignIndex(thirdAngle);
        var thirdBhujja = CalculateBhujja(thirdAngle, thirdSign, reference, stage: 3);
        var thirdShadow = CalculateShadow(thirdBhujja, reference.SecondTableBase, reference.SecondTableDifference);
        var thirdLongitude = thirdSign <= 5
            ? Mod(meanLongitude - thirdShadow, AstrologyConstants.FullCircleArcMinutes)
            : Mod(meanLongitude + thirdShadow, AstrologyConstants.FullCircleArcMinutes);

        var fourthAngle = Mod(solar.MeanLongitudeArcMinutes - thirdLongitude, AstrologyConstants.FullCircleArcMinutes);
        var fourthSign = SignIndex(fourthAngle);
        var fourthBhujja = CalculateBhujja(fourthAngle, fourthSign, reference, stage: 4);
        var fourthShadow = CalculateShadow(fourthBhujja, reference.FirstTableBase, reference.FirstTableDifference);
        return fourthSign <= 5
            ? Mod(thirdLongitude + fourthShadow, AstrologyConstants.FullCircleArcMinutes)
            : Mod(thirdLongitude - fourthShadow, AstrologyConstants.FullCircleArcMinutes);
    }

    private static double CalculateBhujja(
        double angle,
        int sign,
        TraditionalOuterPointReference reference,
        int stage)
    {
        if (!reference.UsesSignBasedBhujja)
        {
            return angle <= 5_400D
                ? angle
                : angle <= 10_800D
                    ? 10_800D - angle
                    : angle <= 16_200D
                        ? angle - 10_800D
                        : 21_600D - angle;
        }

        return stage is 1 or 4
            ? sign <= 2
                ? angle
                : sign <= 5
                    ? 10_800D - angle
                    : sign <= 8
                        ? angle - 10_800D
                        : 21_600D - angle
            : sign <= 5
                ? 10_800D - angle
                : angle - 10_800D;
    }

    private static int CalculateShadow(
        double bhujja,
        IReadOnlyList<int> bases,
        IReadOnlyList<int> differences)
    {
        var segment = Math.Clamp((int)Math.Floor(bhujja / 900D), 0, 6);
        var remainder = Mod(bhujja, 900D);
        var product = remainder * differences[segment];
        return bases[segment]
            + (int)Math.Floor(product / 900D)
            + (Mod(product, 900D) >= 450D ? 1 : 0);
    }

    private static int SignIndex(double longitude) =>
        (int)Math.Floor(longitude / AstrologyConstants.SignArcMinutes);

    private static double Mod(double value, double divisor) =>
        ((value % divisor) + divisor) % divisor;
}
