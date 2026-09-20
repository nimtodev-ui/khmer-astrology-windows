using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Calculation.Suriyayatra;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Infrastructure.ReferenceData;

namespace KhmerAstrology.Tests;

public sealed class TraditionalOuterPointCalculatorTests
{
    [Fact]
    public void TraditionalOuterPointChains_MatchWorkbookSample()
    {
        var input = new BirthInput
        {
            BirthDate = new DateOnly(2026, 8, 21),
            BirthTime = new TimeOnly(4, 9, 12),
            Latitude = 12.25,
            Longitude = 104.66,
            TimeZoneId = "Asia/Phnom_Penh",
        };
        var calendar = new KhmerCalendarCalculator().Calculate(input);
        var solar = new SolarCalculator().Calculate(calendar);
        var result = new TraditionalOuterPointCalculator(
            new JsonTraditionalOuterPointReferenceDataSource()).Calculate(calendar, solar);

        Assert.Equal(3, result.Count);
        AssertLongitude(result, CelestialBody.Mrityu, 2_521D);
        AssertLongitude(result, CelestialBody.Varuna, 20_082D);
        AssertLongitude(result, CelestialBody.Yama, 16_928D);
    }

    private static void AssertLongitude(
        IReadOnlyList<KhmerAstrology.Calculation.Suriyayatra.Models.TraditionalOuterPointPosition> result,
        CelestialBody body,
        double expected)
    {
        var actual = result.Single(position => position.Body == body).LongitudeArcMinutes;
        Assert.InRange(actual, expected - 0.001D, expected + 0.001D);
    }
}
