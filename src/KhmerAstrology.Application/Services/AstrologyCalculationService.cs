using KhmerAstrology.Application.DTOs;
using KhmerAstrology.Application.Interfaces;
using KhmerAstrology.Calculation.Charts;
using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Application.Services;

public sealed class AstrologyCalculationService : IAstrologyCalculationService
{
    private readonly IKhmerCalendarCalculator _khmerCalendarCalculator;
    private readonly ISolarCalculator _solarCalculator;
    private readonly ILunarCalculator _lunarCalculator;
    private readonly IAscendantCalculator _ascendantCalculator;
    private readonly IZodiacCalculator _zodiacCalculator;
    private readonly INakshatraCalculator _nakshatraCalculator;
    private readonly IModernPlanetaryCalculator _modernPlanetaryCalculator;
    private readonly IHouseCalculator _houseCalculator;
    private readonly IDivisionalChartCalculator _d1Calculator;
    private readonly IDivisionalChartCalculator _d3Calculator;
    private readonly IDivisionalChartCalculator _d9Calculator;
    private readonly IInterpretationService _interpretationService;
    private readonly ITraditionalOuterPointCalculator _traditionalOuterPointCalculator;

    public AstrologyCalculationService(
        IKhmerCalendarCalculator khmerCalendarCalculator,
        ISolarCalculator solarCalculator,
        ILunarCalculator lunarCalculator,
        IAscendantCalculator ascendantCalculator,
        IZodiacCalculator zodiacCalculator,
        INakshatraCalculator nakshatraCalculator,
        IModernPlanetaryCalculator modernPlanetaryCalculator,
        IHouseCalculator houseCalculator,
        D1Calculator d1Calculator,
        D3Calculator d3Calculator,
        D9Calculator d9Calculator,
        IInterpretationService interpretationService,
        ITraditionalOuterPointCalculator traditionalOuterPointCalculator)
    {
        _khmerCalendarCalculator = khmerCalendarCalculator;
        _solarCalculator = solarCalculator;
        _lunarCalculator = lunarCalculator;
        _ascendantCalculator = ascendantCalculator;
        _zodiacCalculator = zodiacCalculator;
        _nakshatraCalculator = nakshatraCalculator;
        _modernPlanetaryCalculator = modernPlanetaryCalculator;
        _houseCalculator = houseCalculator;
        _d1Calculator = d1Calculator;
        _d3Calculator = d3Calculator;
        _d9Calculator = d9Calculator;
        _interpretationService = interpretationService;
        _traditionalOuterPointCalculator = traditionalOuterPointCalculator;
    }

    public AstrologyResult Calculate(BirthInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var calendar = _khmerCalendarCalculator.Calculate(input);
        var sun = _solarCalculator.Calculate(calendar);
        var moon = _lunarCalculator.Calculate(calendar, sun);
        var ascendantLongitude = _ascendantCalculator.CalculateLongitudeArcMinutes(input);
        var zodiac = _zodiacCalculator.Calculate(ascendantLongitude);
        var nakshatra = _nakshatraCalculator.Calculate(ascendantLongitude);
        var ascendant = new PlanetPosition
        {
            Body = CelestialBody.Ascendant,
            LongitudeArcMinutes = ascendantLongitude,
            SignNumber = zodiac.SignNumber,
            SignNameEn = zodiac.Sign.NameEn,
            SignNameKm = zodiac.Sign.NameKm,
            Degree = zodiac.Degree,
            Minute = zodiac.Minute,
            Second = zodiac.Second,
            NakshatraNumber = nakshatra.NakshatraNumber,
            NakshatraName = nakshatra.Nakshatra.NameEn,
            Pada = nakshatra.Pada,
            House = 1,
        };
        var modernPositions = _modernPlanetaryCalculator.Calculate(input).Positions
            .Select(position => CreatePlanetPosition(position.Body, position.LongitudeArcMinutes, ascendant.SignNumber))
            .ToArray();
        var additionalPoints = _traditionalOuterPointCalculator.Calculate(calendar, sun)
            .Select(position => CreatePlanetPosition(position.Body, position.LongitudeArcMinutes, ascendant.SignNumber))
            .ToArray();
        var allPositions = new[] { ascendant }.Concat(modernPositions).Concat(additionalPoints).ToArray();
        var interpretations = _interpretationService.Interpret(allPositions);

        var result = new AstrologyResult
        {
            BirthInput = input,
            KhmerCalendar = calendar,
            TraditionalSun = sun,
            TraditionalMoon = moon,
            Ascendant = ascendant,
            Planets = modernPositions,
            AdditionalPoints = additionalPoints,
            D1 = BuildChart(ChartType.D1, allPositions, ascendant, _d1Calculator),
            D3 = BuildChart(ChartType.D3, allPositions, ascendant, _d3Calculator),
            D9 = BuildChart(ChartType.D9, allPositions, ascendant, _d9Calculator),
            Interpretations = interpretations,
            CalculationStage = "Calendar, ascendant, modern planetary result-table, divisional-chart, and house-interpretation chains migrated from workbook references.",
        };
        return result;
    }

    private AstrologyChart BuildChart(
        ChartType chartType,
        IReadOnlyList<PlanetPosition> positions,
        PlanetPosition ascendant,
        IDivisionalChartCalculator calculator)
    {
        var ascendantSign = calculator.CalculateSign(ascendant.LongitudeArcMinutes);
        var houses = Enumerable.Range(1, 12)
            .Select(houseNumber =>
            {
                var signNumber = ((ascendantSign + houseNumber - 2) % 12) + 1;
                var sign = _zodiacCalculator.Calculate((signNumber - 1) * 1800D).Sign;
                return new AstrologyChartHouse
                {
                    HouseNumber = houseNumber,
                    SignNumber = signNumber,
                    SignNameEn = sign.NameEn,
                    SignNameKm = sign.NameKm,
                    Placements = [],
                };
            })
            .ToArray();

        foreach (var position in positions)
        {
            var signNumber = calculator.CalculateSign(position.LongitudeArcMinutes);
            var houseNumber = ((signNumber - ascendantSign + 12) % 12) + 1;
            var house = houses[houseNumber - 1];
            var placements = house.Placements.ToList();
            placements.Add(new AstrologyChartPlacement
            {
                Body = position.Body,
                DisplayName = position.Body == CelestialBody.Ascendant ? "Asc" : position.Body.ToString(),
            });
            houses[houseNumber - 1] = new AstrologyChartHouse
            {
                HouseNumber = house.HouseNumber,
                SignNumber = house.SignNumber,
                SignNameEn = house.SignNameEn,
                SignNameKm = house.SignNameKm,
                Placements = placements,
            };
        }

        return new AstrologyChart { ChartType = chartType, Houses = houses };
    }

    private PlanetPosition CreatePlanetPosition(
        CelestialBody body,
        double longitudeArcMinutes,
        int ascendantSign)
    {
        var zodiac = _zodiacCalculator.Calculate(longitudeArcMinutes);
        var nakshatra = _nakshatraCalculator.Calculate(longitudeArcMinutes);
        return new PlanetPosition
        {
            Body = body,
            LongitudeArcMinutes = longitudeArcMinutes,
            SignNumber = zodiac.SignNumber,
            SignNameEn = zodiac.Sign.NameEn,
            SignNameKm = zodiac.Sign.NameKm,
            Degree = zodiac.Degree,
            Minute = zodiac.Minute,
            Second = zodiac.Second,
            NakshatraNumber = nakshatra.NakshatraNumber,
            NakshatraName = nakshatra.Nakshatra.NameEn,
            Pada = nakshatra.Pada,
            House = _houseCalculator.CalculateWholeSignHouse(ascendantSign, zodiac.SignNumber),
        };
    }
}
