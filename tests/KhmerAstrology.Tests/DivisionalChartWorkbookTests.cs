using KhmerAstrology.Application.Interfaces;
using KhmerAstrology.Calculation.Charts;
using KhmerAstrology.Calculation.Zodiac;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace KhmerAstrology.Tests;

/// <summary>
/// Cached values from the attached workbook sheet <c>រាសិចក្ក D1-D9-D3!A5:F18</c>
/// (sample 28/10/2024 20:00:12, Phnom Penh). Column C is the longitude and
/// columns D/E/F are the D1/D9/D3 signs. The sheet rows coded ០/វ/យ read
/// <c>សូរ្យយាត្រ!B28:B30</c>, which are the Uranus/Neptune/Pluto rows here.
/// </summary>
public sealed class DivisionalChartWorkbookTests
{
    public static TheoryData<CelestialBody, double, int, int, int> WorkbookRows => new()
    {
        { CelestialBody.Ascendant, 2_961.5007818897216D, 2, 3, 6 },
        { CelestialBody.Sun, 11_483.728512255722D, 7, 10, 11 },
        { CelestialBody.Moon, 8_891.5565868411722D, 5, 9, 1 },
        { CelestialBody.Mars, 5_602.4635633703701D, 4, 5, 4 },
        { CelestialBody.Mercury, 12_498.217087363319D, 7, 3, 3 },
        { CelestialBody.Jupiter, 3_391.1107725866577D, 2, 5, 10 },
        { CelestialBody.Venus, 13_724.517315768433D, 8, 9, 12 },
        { CelestialBody.Saturn, 19_126.331729715519D, 11, 12, 3 },
        { CelestialBody.Rahu, 20_536.560994679141D, 12, 7, 4 },
        { CelestialBody.KetuVeda, 9_736.560994679141D, 6, 1, 10 },
        { CelestialBody.KetuDivya, 13_780.579209636036D, 8, 9, 12 },
        { CelestialBody.Uranus, 1_909.9003595359013D, 2, 10, 2 },
        { CelestialBody.Neptune, 20_001.957351659843D, 12, 5, 12 },
        { CelestialBody.Pluto, 16_530.243119455332D, 10, 11, 10 },
    };

    [Theory]
    [MemberData(nameof(WorkbookRows))]
    public void Calculators_MatchWorkbookSignColumns(
        CelestialBody body,
        double longitudeArcMinutes,
        int expectedD1,
        int expectedD9,
        int expectedD3)
    {
        var normalizer = new LongitudeNormalizer();

        Assert.True(expectedD1 == new D1Calculator(normalizer).CalculateSign(longitudeArcMinutes), $"{body} D1");
        Assert.True(expectedD9 == new D9Calculator(normalizer).CalculateSign(longitudeArcMinutes), $"{body} D9");
        Assert.True(expectedD3 == new D3Calculator(normalizer).CalculateSign(longitudeArcMinutes), $"{body} D3");
    }

    [Fact]
    public void Charts_PlaceEveryWorkbookBodyInItsDivisionalSign()
    {
        using var provider = new ServiceCollection().AddKhmerAstrologyServices().BuildServiceProvider();
        var result = provider.GetRequiredService<IAstrologyCalculationService>().Calculate(new BirthInput
        {
            Name = "Attached Khmer workbook sample",
            BirthDate = new DateOnly(2024, 10, 28),
            BirthTime = new TimeOnly(20, 0, 12),
            Country = "Cambodia",
            Province = "Phnom Penh",
            Latitude = 11.55,
            Longitude = 104.92,
            TimeZoneId = "Asia/Phnom_Penh",
        });

        foreach (var row in WorkbookRows)
        {
            var body = (CelestialBody)row[0];
            AssertPlacedInSign(result.D1, body, (int)row[2]);
            AssertPlacedInSign(result.D9, body, (int)row[3]);
            AssertPlacedInSign(result.D3, body, (int)row[4]);
        }
    }

    [Fact]
    public void Charts_StartHouseOneAtTheAscendantSignAndCarryD1Longitudes()
    {
        using var provider = new ServiceCollection().AddKhmerAstrologyServices().BuildServiceProvider();
        var result = provider.GetRequiredService<IAstrologyCalculationService>().Calculate(new BirthInput
        {
            Name = "Attached Khmer workbook sample",
            BirthDate = new DateOnly(2024, 10, 28),
            BirthTime = new TimeOnly(20, 0, 12),
            Latitude = 11.55,
            Longitude = 104.92,
            TimeZoneId = "Asia/Phnom_Penh",
        });

        foreach (var chart in new[] { result.D1, result.D9, result.D3 })
        {
            var ascendantHouse = Assert.Single(chart.Houses, house =>
                house.Placements.Any(placement => placement.Body == CelestialBody.Ascendant));
            Assert.Equal(1, ascendantHouse.HouseNumber);
            Assert.Equal(
                Enumerable.Range(0, 12).Select(offset => (chart.Houses[0].SignNumber - 1 + offset) % 12 + 1),
                chart.Houses.Select(house => house.SignNumber));
        }

        var sun = result.D9.Houses.SelectMany(house => house.Placements).Single(placement => placement.Body == CelestialBody.Sun);
        Assert.Equal(result.Planets.Single(planet => planet.Body == CelestialBody.Sun).LongitudeArcMinutes, sun.LongitudeArcMinutes);
    }

    private static void AssertPlacedInSign(AstrologyChart chart, CelestialBody body, int expectedSign)
    {
        var house = Assert.Single(chart.Houses, candidate => candidate.Placements.Any(placement => placement.Body == body));
        Assert.True(expectedSign == house.SignNumber, $"{chart.ChartType} {body}: expected sign {expectedSign}, got {house.SignNumber}");
    }
}
