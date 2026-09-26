using KhmerAstrology.Application.Services;
using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Calculation.Charts;
using KhmerAstrology.Calculation.Houses;
using KhmerAstrology.Calculation.Suriyayatra;
using KhmerAstrology.Calculation.Zodiac;
using KhmerAstrology.Infrastructure.ReferenceData;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Tests;

public sealed class ProductionCalculationPathTests
{
    [Fact]
    public void ProductionReferencePath_ReturnsWorkbookBackedResult()
    {
        var service = CreateProductionService();

        var result = service.Calculate(new BirthInput
        {
            Name = "Production path",
            BirthDate = new DateOnly(2026, 8, 21),
            BirthTime = new TimeOnly(4, 9, 12),
            Country = "Cambodia",
            Province = "Kampong Chhnang",
            Latitude = 12.25,
            Longitude = 104.66,
            TimeZoneId = "Asia/Phnom_Penh",
        });

        Assert.Equal("Cancer", result.Ascendant.SignNameEn);
        Assert.Equal("Pushya", result.Ascendant.NakshatraName);
        Assert.Equal(13, result.Planets.Count);
        Assert.Equal(3, result.AdditionalPoints.Count);
        Assert.Equal("Second Asalha", result.KhmerCalendar.LunarMonthNameEn);
        Assert.Equal("Friday", result.KhmerCalendar.Weekday);
        Assert.Equal(12, result.D1.Houses.Count);
        Assert.Equal(12, result.D3.Houses.Count);
        Assert.Equal(12, result.D9.Houses.Count);
        Assert.Contains(result.Planets, planet => planet.Body == CelestialBody.KetuDivya);
        Assert.Equal(17, result.Interpretations.Count);
    }

    [Fact]
    public void ProductionReferencePath_MatchesAttachedKhmerWorkbookSample()
    {
        var result = CreateProductionService().Calculate(new BirthInput
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

        var expected = new Dictionary<CelestialBody, double>
        {
            [CelestialBody.Ascendant] = 2_961.5007818897216D,
            [CelestialBody.Sun] = 11_483.728512255722D,
            [CelestialBody.Moon] = 8_891.5565868411722D,
            [CelestialBody.Mars] = 5_602.4635633703701D,
            [CelestialBody.Mercury] = 12_498.217087363319D,
            [CelestialBody.Jupiter] = 3_391.1107725866577D,
            [CelestialBody.Venus] = 13_724.517315768433D,
            [CelestialBody.Saturn] = 19_126.331729715519D,
            [CelestialBody.Rahu] = 20_536.560994679141D,
            [CelestialBody.KetuVeda] = 9_736.560994679141D,
            [CelestialBody.KetuDivya] = 13_780.579209636036D,
            [CelestialBody.Uranus] = 1_909.9003595359013D,
            [CelestialBody.Neptune] = 20_001.957351659843D,
            [CelestialBody.Pluto] = 16_530.243119455332D,
        };

        Assert.InRange(
            result.Ascendant.LongitudeArcMinutes,
            expected[CelestialBody.Ascendant] - 0.000001D,
            expected[CelestialBody.Ascendant] + 0.000001D);

        foreach (var position in result.Planets)
        {
            Assert.True(expected.ContainsKey(position.Body), $"Unexpected body: {position.Body}");
            Assert.InRange(
                position.LongitudeArcMinutes,
                expected[position.Body] - 0.000001D,
                expected[position.Body] + 0.000001D);
        }

        Assert.Equal(3, result.AdditionalPoints.Count);
    }

    private static AstrologyCalculationService CreateProductionService()
    {
        var normalizer = new LongitudeNormalizer();
        return new AstrologyCalculationService(
            new KhmerCalendarCalculator(new JsonKhmerCalendarMonthReferenceDataSource()),
            new SolarCalculator(),
            new LunarCalculator(),
            new AscendantCalculator(),
            new ZodiacCalculator(normalizer, new JsonZodiacReferenceDataSource()),
            new NakshatraCalculator(normalizer, new JsonNakshatraReferenceDataSource()),
            new ModernPlanetaryCalculator(new JsonModernPlanetaryReferenceDataSource()),
            new HouseCalculator(),
            new D1Calculator(normalizer),
            new D3Calculator(normalizer),
            new D9Calculator(normalizer),
            new InterpretationService(new JsonInterpretationReferenceDataSource(), new JsonCelestialBodyReferenceDataSource()),
            new TraditionalOuterPointCalculator(new JsonTraditionalOuterPointReferenceDataSource()));
    }
}
