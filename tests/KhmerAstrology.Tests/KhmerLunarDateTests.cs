using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Infrastructure.ReferenceData;

namespace KhmerAstrology.Tests;

/// <summary>
/// The birth-date lunar fields must follow the workbook's day-by-day lunar walk
/// (sheets 30/31). These festival dates are fixed by the lunar calendar itself.
/// </summary>
public sealed class KhmerLunarDateTests
{
    private static readonly JsonKhmerCalendarYearReferenceDataSource YearCodes = new();
    private static readonly KhmerCalendarCalculator Calendar = new(YearCodes);
    private static readonly AutomaticCalendarCalculator Automatic = new(YearCodes);

    [Theory]
    [InlineData(2024, 2, 24, "15 Waxing", "Magha", "Full Moon")]        // Meak Bochea
    [InlineData(2024, 5, 22, "15 Waxing", "Visakha", "Full Moon")]      // Visak Bochea
    [InlineData(2024, 10, 2, "15 Waning", "Bhadrapada", "New Moon")]    // Pchum Ben
    [InlineData(2024, 10, 17, "15 Waxing", "Assayuja", "Full Moon")]    // End of Buddhist Lent
    [InlineData(2025, 5, 11, "15 Waxing", "Visakha", "Full Moon")]      // Visak Bochea 2025
    public void FestivalDates_HaveTheirLunarDay(int year, int month, int day, string lunarDay, string lunarMonth, string tithi)
    {
        var result = Calendar.Calculate(new BirthInput { BirthDate = new DateOnly(year, month, day), BirthTime = new TimeOnly(12, 0) });

        Assert.Equal(lunarDay, result.LunarDayDisplay);
        Assert.Equal(lunarMonth, result.LunarMonthNameEn);
        Assert.Equal(tithi, result.TithiName);
    }

    [Theory]
    [InlineData(2024)]
    [InlineData(2025)]
    [InlineData(2026)]
    [InlineData(2027)]
    public void BirthCalendar_AgreesWithTheAutomaticCalendarOnEveryDay(int year)
    {
        for (var date = new DateOnly(year, 1, 1); date.Year == year; date = date.AddDays(1))
        {
            var birth = Calendar.Calculate(new BirthInput { BirthDate = date, BirthTime = new TimeOnly(12, 0) });
            var state = Automatic.GetLunarState(date.Year, date.Month, date.Day);
            var row = Automatic.SearchDate(date.Year, date.Month, date.Day);

            Assert.True(state.LunarDay == birth.LunarDay, $"{date}: lunar day {birth.LunarDay} vs walk {state.LunarDay}");
            Assert.Equal(state.LunarMonthIndex, birth.LunarMonthNumber);
            Assert.StartsWith(AutomaticCalendarCalculator.ToKhmerNumerals(state.DayInPhase), row.LunarDay);
        }
    }

    [Fact]
    public void NewMoon_CoversDays29And30LikeWorkbookK6()
    {
        Assert.True(new KhmerLunarState(1386, 1, 1, 29, 29).IsNewMoon);
        Assert.True(new KhmerLunarState(1386, 1, 2, 30, 30).IsNewMoon);
        Assert.False(new KhmerLunarState(1386, 1, 2, 28, 30).IsNewMoon);
    }
}
