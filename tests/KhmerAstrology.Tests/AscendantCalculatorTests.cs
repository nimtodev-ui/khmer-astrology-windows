using KhmerAstrology.Calculation.Suriyayatra;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Tests;

public sealed class AscendantCalculatorTests
{
    [Fact]
    public void Ascendant_MatchesWorkbookModernChainSample()
    {
        var result = new AscendantCalculator().CalculateLongitudeArcMinutes(new BirthInput
        {
            BirthDate = new DateOnly(2026, 8, 21),
            BirthTime = new TimeOnly(4, 9, 12),
            Latitude = 12.25,
            Longitude = 104.66,
            TimeZoneId = "Asia/Phnom_Penh",
        });

        Assert.InRange(result, 5_942.19, 5_942.21);
    }

    [Fact]
    public void Ascendant_IntermediateChain_MatchesKhmerWorkbookSample()
    {
        var result = new AscendantCalculator().Calculate(new BirthInput
        {
            BirthDate = new DateOnly(2024, 10, 28),
            BirthTime = new TimeOnly(20, 0, 12),
            Latitude = 11.55,
            Longitude = 104.92,
            TimeZoneId = "Asia/Phnom_Penh",
        });

        Assert.InRange(result.UtcExcelSerial, 45_593.54180, 45_593.54181);
        Assert.InRange(result.UtcJulianDay, 2_460_612.04180, 2_460_612.04181);
        Assert.InRange(result.JulianCenturies, 0.24824207, 0.24824208);
        Assert.InRange(result.MeanSiderealTimeDegrees, 232.41651, 232.41653);
        Assert.InRange(result.LocalSiderealTimeDegrees, 337.33651, 337.33653);
        Assert.InRange(result.MeanObliquityDegrees, 23.43606, 23.43607);
        Assert.InRange(result.TropicalAscendantDegrees, 73.56222, 73.56223);
        Assert.InRange(result.LahiriAyanamsaDegrees, 24.20387, 24.20388);
        Assert.InRange(result.SiderealAscendantDegrees, 49.35834, 49.35835);
        Assert.InRange(result.SiderealAscendantArcMinutes, 2_961.5007, 2_961.5009);
    }

    [Fact]
    public void Ascendant_UsesWorkbookUtcOverrideWhenProvided()
    {
        var calculator = new AscendantCalculator();
        var baseInput = new BirthInput
        {
            BirthDate = new DateOnly(2024, 10, 28),
            BirthTime = new TimeOnly(20, 0, 12),
            Latitude = 11.55,
            Longitude = 104.92,
            TimeZoneId = "Asia/Phnom_Penh",
        };

        var overridden = calculator.Calculate(new BirthInput
        {
            BirthDate = baseInput.BirthDate,
            BirthTime = baseInput.BirthTime,
            Latitude = baseInput.Latitude,
            Longitude = baseInput.Longitude,
            TimeZoneId = baseInput.TimeZoneId,
            UtcOffsetOverrideHours = 0D,
        });
        var localZone = calculator.Calculate(baseInput);

        Assert.InRange(
            overridden.UtcExcelSerial - localZone.UtcExcelSerial,
            7D / 24D - 0.000001D,
            7D / 24D + 0.000001D);
    }

    [Theory]
    [InlineData(-91D, 104.9D)]
    [InlineData(91D, 104.9D)]
    [InlineData(11.5D, -181D)]
    [InlineData(11.5D, 181D)]
    public void Ascendant_RejectsInvalidCoordinates(double latitude, double longitude)
    {
        var input = new BirthInput
        {
            BirthDate = new DateOnly(2024, 10, 28),
            BirthTime = new TimeOnly(20, 0),
            Latitude = latitude,
            Longitude = longitude,
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AscendantCalculator().Calculate(input));
    }
}
