using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Infrastructure.ReferenceData;

namespace KhmerAstrology.Tests;

public sealed class KhmerCalendarCalculatorTests
{
    private readonly KhmerCalendarCalculator _calculator = new();

    [Fact]
    public void Calendar_MatchesWorkbookAtthabhujjaSample()
    {
        var result = _calculator.Calculate(new BirthInput
        {
            BirthDate = new DateOnly(2026, 8, 21),
            BirthTime = new TimeOnly(4, 9, 12),
            Latitude = 12.25,
            Longitude = 104.67,
            TimeZoneId = "Asia/Phnom_Penh",
        });

        Assert.Equal(2026, result.AstronomicalYear);
        Assert.Equal(1388, result.KhmerYear);
        Assert.Equal(2570, result.BuddhistYear);
        Assert.Equal(507_107, result.AharganaDay);
        Assert.InRange(result.Ahargana, 507_107.17305555, 507_107.17305557);
        Assert.InRange(result.Kammaja, 661.55555554, 661.55555557);
        Assert.Equal(489, result.SolarYearFraction);
        Assert.Equal(615, result.Avamana);
        Assert.Equal(17_172, result.Masakendra);
        Assert.Equal(8, result.BoriTithi);
        Assert.Equal("Common Year", result.YearType);
        Assert.Equal(365, result.DaysInYear);
        Assert.Equal("12-Month Lunar Year", result.LunarYearType);
        Assert.Equal(6, result.NewEraDay);
        Assert.Equal("Thursday", result.NewEraWeekday);
        Assert.Equal(16, result.MahaSankrantaDay);
        Assert.Equal(TimeSpan.FromSeconds(52_812), result.MahaSankrantaTime);
        Assert.Equal(14, result.NextMahaSankrantaDay);
        Assert.Equal(TimeSpan.FromSeconds(38_556), result.NextMahaSankrantaTime);
        Assert.Equal("Tuesday", result.NextMahaSankrantaWeekday);
    }

    [Fact]
    public void AtthabhujjYearInput_MatchesWorkbookSampleWithoutBirthInput()
    {
        var result = _calculator.CalculateForYear(
            2026,
            21,
            8,
            new TimeOnly(4, 9, 12));

        Assert.Equal(2026, result.AstronomicalYear);
        Assert.Equal(1388, result.KhmerYear);
        Assert.Equal(507_107, result.AharganaDay);
        Assert.Equal(615, result.Avamana);
        Assert.Equal(17_172, result.Masakendra);
        Assert.Equal(8, result.BoriTithi);
    }

    [Fact]
    public void AtthabhujjYearInput_AcceptsNegativeBceYear()
    {
        var result = _calculator.CalculateForYear(-2026, 21, 8, TimeOnly.MinValue);

        Assert.Equal(-2025, result.AstronomicalYear);
        Assert.Equal(-2663, result.KhmerYear);
    }

    [Fact]
    public void Calendar_UsesExcelCompatiblePositiveModulo()
    {
        var result = _calculator.Calculate(new BirthInput
        {
            BirthDate = new DateOnly(1, 1, 1),
            BirthTime = TimeOnly.MinValue,
        });

        Assert.InRange(result.Kammaja, 0, 800);
        Assert.InRange(result.Avamana, 0, 691);
        Assert.InRange(result.BoriTithi, 0, 29);
        Assert.InRange(result.NewEraDay, 0, 6);
    }

    [Fact]
    public void CalendarDateDetails_MatchWorkbookAutomaticCalendarSample()
    {
        var calculator = new KhmerCalendarCalculator(new JsonKhmerCalendarMonthReferenceDataSource());
        var result = calculator.Calculate(new BirthInput
        {
            BirthDate = new DateOnly(2026, 1, 1),
            BirthTime = TimeOnly.MinValue,
        });

        Assert.Equal("Thursday", result.Weekday);
        Assert.Equal(2569, result.BuddhistYear);
        Assert.Equal(13, result.LunarDay);
        Assert.Equal("13 Waxing", result.LunarDayDisplay);
        Assert.Equal("Jaya Tithi", result.TithiName);
        Assert.Equal(2, result.LunarMonthNumber);
        Assert.Equal("Phussa", result.LunarMonthNameEn);
        Assert.Equal("Snake", result.AnimalYear);
        Assert.Equal(1, result.SesaKalaYoga);
        Assert.Equal(39, result.Yuga);
    }

    [Fact]
    public void BuddhistYear_ChangesOnWorkbookDayOfYearBoundary()
    {
        var calculator = new KhmerCalendarCalculator(new JsonKhmerCalendarMonthReferenceDataSource());

        var dayBeforeBoundary = calculator.Calculate(new BirthInput
        {
            BirthDate = new DateOnly(2024, 4, 15),
            BirthTime = TimeOnly.MinValue,
        });
        var boundaryDay = calculator.Calculate(new BirthInput
        {
            BirthDate = new DateOnly(2024, 4, 16),
            BirthTime = TimeOnly.MinValue,
        });

        Assert.Equal(2567, dayBeforeBoundary.BuddhistYear);
        Assert.Equal(2568, boundaryDay.BuddhistYear);
    }
}
