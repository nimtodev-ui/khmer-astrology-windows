using KhmerAstrology.Calculation.Suriyayatra;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Infrastructure.ReferenceData;

namespace KhmerAstrology.Tests;

public sealed class ModernPlanetaryCalculatorTests
{
    [Fact]
    public void ModernPlanetaryChain_MatchesWorkbookMainResultTable()
    {
        var result = new ModernPlanetaryCalculator(new JsonModernPlanetaryReferenceDataSource()).Calculate(new BirthInput
        {
            BirthDate = new DateOnly(2026, 8, 21),
            BirthTime = new TimeOnly(4, 9, 12),
            Latitude = 12.25,
            Longitude = 104.66,
            TimeZoneId = "Asia/Phnom_Penh",
        });

        Assert.Equal(13, result.Positions.Count);
        AssertLongitude(result, CelestialBody.Sun, 7_418.1025339840035D);
        AssertLongitude(result, CelestialBody.Moon, 13_324.781560153069D);
        AssertLongitude(result, CelestialBody.Mars, 4_321.3374349146325D);
        AssertLongitude(result, CelestialBody.Mercury, 6_994.2598918044505D);
        AssertLongitude(result, CelestialBody.Jupiter, 6_425.7319357357292D);
        AssertLongitude(result, CelestialBody.Venus, 10_162.442402280327D);
        AssertLongitude(result, CelestialBody.Saturn, 20_999.551657663338D);
        AssertLongitude(result, CelestialBody.Rahu, 18_335.690345607556D);
        AssertLongitude(result, CelestialBody.KetuVeda, 7_535.6903456075561D);
        AssertLongitude(result, CelestialBody.KetuDivya, 14_342.383333339476D);
        AssertLongitude(result, CelestialBody.Uranus, 2_476.7955940231541D);
        AssertLongitude(result, CelestialBody.Neptune, 20_381.266457406655D);
        AssertLongitude(result, CelestialBody.Pluto, 16_769.90424895427D);
    }

    private static void AssertLongitude(
        KhmerAstrology.Calculation.Suriyayatra.Models.ModernPlanetaryCalculationResult result,
        CelestialBody body,
        double expected)
    {
        var actual = result.Positions.Single(position => position.Body == body).LongitudeArcMinutes;
        Assert.InRange(actual, expected - 0.001D, expected + 0.001D);
    }
}
