using KhmerAstrology.Application.Services;
using KhmerAstrology.Application.Interfaces;
using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Calculation.Charts;
using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Calculation.Suriyayatra;
using KhmerAstrology.Calculation.Zodiac;
using KhmerAstrology.Calculation.Houses;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Calculation.Suriyayatra.Models;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Tests;

public sealed class AstrologyCalculationServiceTests
{
    [Fact]
    public void Calculate_ReturnsUserSpecificCalendarAndAscendant()
    {
        var normalizer = new LongitudeNormalizer();
        var service = new AstrologyCalculationService(
            new KhmerCalendarCalculator(),
            new SolarCalculator(),
            new LunarCalculator(),
            new AscendantCalculator(),
            new ZodiacCalculator(normalizer),
            new NakshatraCalculator(normalizer),
            new StubModernPlanetaryCalculator(),
            new HouseCalculator(),
            new D1Calculator(normalizer),
            new D3Calculator(normalizer),
            new D9Calculator(normalizer),
            new StubInterpretationService(),
            new StubTraditionalOuterPointCalculator());

        var result = service.Calculate(new BirthInput
        {
            Name = "Workbook sample",
            BirthDate = new DateOnly(2026, 8, 21),
            BirthTime = new TimeOnly(4, 9, 12),
            Latitude = 12.25,
            Longitude = 104.66,
            TimeZoneId = "Asia/Phnom_Penh",
        });

        Assert.Equal(1_388, result.KhmerCalendar.KhmerYear);
        Assert.InRange(result.Ascendant.LongitudeArcMinutes, 5_942.19, 5_942.21);
        Assert.Equal("Cancer", result.Ascendant.SignNameEn);
        Assert.Equal("Pushya", result.Ascendant.NakshatraName);
        Assert.Equal(2, result.Ascendant.Pada);
        Assert.Equal(12, result.D1.Houses.Count);
        Assert.Contains(result.D1.Houses.SelectMany(house => house.Placements), placement => placement.Body == CelestialBody.Ascendant);
    }

    private sealed class StubModernPlanetaryCalculator : IModernPlanetaryCalculator
    {
        public ModernPlanetaryCalculationResult Calculate(BirthInput input) =>
            new(0, [new ModernPlanetaryPosition(CelestialBody.Sun, 7_418.1025339840035D)]);
    }

    private sealed class StubInterpretationService : IInterpretationService
    {
        public IReadOnlyList<InterpretationResult> Interpret(IReadOnlyList<PlanetPosition> positions) => [];
    }

    private sealed class StubTraditionalOuterPointCalculator : ITraditionalOuterPointCalculator
    {
        public IReadOnlyList<TraditionalOuterPointPosition> Calculate(
            KhmerCalendarResult calendar,
            SolarCalculationResult solar) => [];
    }
}
