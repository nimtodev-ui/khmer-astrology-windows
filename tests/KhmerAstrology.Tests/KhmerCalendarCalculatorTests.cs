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
        Assert.Equal(14, result.MahaSankrantaDay);
        Assert.Equal(4, result.MahaSankrantaMonth);
        Assert.Equal(TimeSpan.FromSeconds(38_556), result.MahaSankrantaTime);
        Assert.Equal("Tuesday", result.MahaSankrantaWeekday);
        Assert.Equal(16, result.RiseOfSakDay);
        Assert.Equal(4, result.RiseOfSakMonth);
        Assert.Equal(TimeSpan.FromSeconds(52_812), result.RiseOfSakTime);
        Assert.Equal("Thursday", result.RiseOfSakWeekday);
        Assert.Equal(16, result.NextMahaSankrantaDay);
        Assert.Equal(TimeSpan.FromSeconds(52_812), result.NextMahaSankrantaTime);
        Assert.Equal("Thursday", result.NextMahaSankrantaWeekday);
    }

    [Fact]
    public void Calendar_MatchesWorkbook2027AtthabhujjSample()
    {
        var result = _calculator.CalculateForYear(2027, 21, 8, TimeOnly.MinValue);

        Assert.Equal(2027, result.AstronomicalYear);
        Assert.Equal(1389, result.KhmerYear);
        Assert.Equal(14, result.MahaSankrantaDay);
        Assert.Equal(4, result.MahaSankrantaMonth);
        Assert.Equal(TimeSpan.FromSeconds(60_912), result.MahaSankrantaTime);
        Assert.Equal("Wednesday", result.MahaSankrantaWeekday);
        Assert.Equal(16, result.RiseOfSakDay);
        Assert.Equal(4, result.RiseOfSakMonth);
        Assert.Equal(TimeSpan.FromSeconds(75_168), result.RiseOfSakTime);
        Assert.Equal("Friday", result.RiseOfSakWeekday);
    }

    [Theory]
    [InlineData(2024, 13, 22, 17, 24, "Saturday", 16, 2, 15, 0, "Tuesday")]
    [InlineData(2025, 14, 4, 30, 0, "Monday", 16, 8, 27, 36, "Wednesday")]
    [InlineData(2026, 14, 10, 42, 36, "Tuesday", 16, 14, 40, 12, "Thursday")]
    [InlineData(2027, 14, 16, 55, 12, "Wednesday", 16, 20, 52, 48, "Friday")]
    [InlineData(2028, 13, 23, 7, 48, "Thursday", 16, 3, 5, 24, "Sunday")]
    public void Calendar_SankrantaAndRiseOfSak_MatchOfficialKhmerAlmanacAcrossYears(
        int year,
        int sanDay, int sanH, int sanM, int sanS, string sanWeekday,
        int sakDay, int sakH, int sakM, int sakS, string sakWeekday)
    {
        var result = _calculator.CalculateForYear(year, 1, 1, TimeOnly.MinValue);

        Assert.Equal(sanDay, result.MahaSankrantaDay);
        Assert.Equal(new TimeSpan(sanH, sanM, sanS), result.MahaSankrantaTime);
        Assert.Equal(sanWeekday, result.MahaSankrantaWeekday);

        Assert.Equal(sakDay, result.RiseOfSakDay);
        Assert.Equal(new TimeSpan(sakH, sakM, sakS), result.RiseOfSakTime);
        Assert.Equal(sakWeekday, result.RiseOfSakWeekday);
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

    [Theory]
    [InlineData("2026", true, 2026)]
    [InlineData("  2027  ", true, 2027)]
    [InlineData("+2026", true, 2026)]
    [InlineData("២០២៦", true, 2026)]
    [InlineData("២០២៧", true, 2027)]
    [InlineData("-500", true, -500)]
    [InlineData("−500", true, -500)] // Unicode minus U+2212
    [InlineData("–500", true, -500)] // En-dash U+2013
    [InlineData("—500", true, -500)] // Em-dash U+2014
    [InlineData("-៥០០", true, -500)]
    [InlineData("500 មុន គ.ស.", true, -500)]
    [InlineData("២០២៦ គ.ស.", true, 2026)]
    [InlineData("0", false, 0)]
    [InlineData("០", false, 0)]
    [InlineData("", false, 0)]
    [InlineData("   ", false, 0)]
    [InlineData("invalid", false, 0)]
    public void TryParseYear_ParsesVariousFormats(string input, bool expectedSuccess, int expectedYear)
    {
        var success = KhmerCalendarCalculator.TryParseYear(input, out var year);
        Assert.Equal(expectedSuccess, success);
        if (expectedSuccess)
        {
            Assert.Equal(expectedYear, year);
        }
    }
}
