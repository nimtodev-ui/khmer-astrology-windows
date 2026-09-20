using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Calculation.Suriyayatra;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Tests;

public sealed class TraditionalPlanetaryCalculatorTests
{
    private readonly KhmerCalendarResult _calendar = new KhmerCalendarCalculator().Calculate(new BirthInput
    {
        BirthDate = new DateOnly(2026, 8, 21),
        BirthTime = new TimeOnly(4, 9, 12),
    });

    [Fact]
    public void SolarCalculator_MatchesTranslatedWorkbookChain()
    {
        var result = new SolarCalculator().Calculate(_calendar);

        Assert.InRange(result.MeanRemainder, 102_572.5555, 102_572.5557);
        Assert.Equal(7_582, result.MeanLongitudeArcMinutes);
        Assert.Equal(7_527, result.LongitudeArcMinutes);
    }

    [Fact]
    public void LunarCalculator_MatchesTranslatedWorkbookChain()
    {
        var sun = new SolarCalculator().Calculate(_calendar);
        var result = new LunarCalculator().Calculate(_calendar, sun);

        Assert.Equal(13_941, result.MeanLongitudeArcMinutes);
        Assert.Equal(14_057, result.LongitudeArcMinutes);
    }
}
