using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Infrastructure.ReferenceData;

namespace KhmerAstrology.Tests;

public sealed class KhmerLunarCalendarLayoutTests
{
    private static readonly AutomaticCalendarCalculator Automatic = new(new JsonKhmerCalendarYearReferenceDataSource());

    [Theory]
    [InlineData("2026-09-26", "2026-09-20")] // Saturday → preceding Sunday
    [InlineData("2026-09-20", "2026-09-20")] // Sunday stays
    [InlineData("2026-01-01", "2025-12-28")] // crosses the year
    [InlineData("0001-01-03", "0001-01-01")] // clamped to DateTime.MinValue
    public void WeekStart_IsTheSundayOnOrBefore(string date, string expected)
    {
        Assert.Equal(DateTime.Parse(expected), KhmerLunarCalendarLayout.WeekStart(DateTime.Parse(date)));
    }

    [Theory]
    [InlineData(2026, 2, 4)]  // Feb 2026 starts on Sunday: 28 days, 4 rows
    [InlineData(2026, 9, 5)]
    [InlineData(2026, 8, 6)]  // Aug 2026 starts on Saturday: 31 days, 6 rows
    public void MonthWeekRows_CountsTheRowsTheMonthUses(int year, int month, int expected)
    {
        Assert.Equal(expected, KhmerLunarCalendarLayout.MonthWeekRows(year, month));
    }

    [Fact]
    public void Step_MovesByTheViewPeriodAndClampsAtTheDateTimeRange()
    {
        var date = new DateTime(2026, 1, 31);
        Assert.Equal(new DateTime(2026, 2, 7), KhmerLunarCalendarLayout.Step(date, KhmerLunarCalendarView.Week, 1));
        Assert.Equal(new DateTime(2026, 2, 28), KhmerLunarCalendarLayout.Step(date, KhmerLunarCalendarView.Month, 1));
        Assert.Equal(new DateTime(2025, 1, 31), KhmerLunarCalendarLayout.Step(date, KhmerLunarCalendarView.Year, -1));
        Assert.Equal(DateTime.MinValue, KhmerLunarCalendarLayout.Step(new DateTime(1, 1, 5), KhmerLunarCalendarView.Week, -1));
    }

    [Theory]
    [InlineData(8, 30, KhmerMoonPhase.FirstQuarter)]
    [InlineData(15, 29, KhmerMoonPhase.Full)]
    [InlineData(23, 29, KhmerMoonPhase.LastQuarter)]
    [InlineData(29, 29, KhmerMoonPhase.New)]
    [InlineData(29, 30, KhmerMoonPhase.None)]
    [InlineData(30, 30, KhmerMoonPhase.New)]
    [InlineData(1, 30, KhmerMoonPhase.None)]
    public void MoonPhase_MarksEveryHolyDay(int lunarDay, int monthLength, KhmerMoonPhase expected)
    {
        Assert.Equal(expected, KhmerLunarCalendarRules.GetMoonPhase(lunarDay, monthLength));
        Assert.Equal(expected != KhmerMoonPhase.None, KhmerLunarCalendarRules.IsHolyDay(lunarDay, monthLength));
    }

    [Theory]
    [InlineData(8, true, "៨កើត")]
    [InlineData(15, true, "១៥កើត")]
    [InlineData(23, true, "៨រោច")]
    [InlineData(23, false, "Wane 8")]
    [InlineData(1, false, "Wax 1")]
    public void FormatLunarDay_UsesWaxingAndWaningHalves(int lunarDay, bool khmer, string expected)
    {
        Assert.Equal(expected, KhmerLunarCalendarRules.FormatLunarDay(lunarDay, khmer));
    }

    [Fact]
    public void LunarNames_AreRomanisedInEnglishAndKeptInKhmer()
    {
        Assert.Equal("Photrobot", KhmerLunarCalendarRules.GetLunarMonthName("ខែភទ្របទ", khmer: false));
        Assert.Equal("ខែភទ្របទ", KhmerLunarCalendarRules.GetLunarMonthName("ខែភទ្របទ", khmer: true));
        Assert.Equal("Horse", KhmerLunarCalendarRules.GetAnimalYearName("មមី", khmer: false));
    }

    // Cambodian public-holiday dates published for 2024 and 2025.
    [Theory]
    [InlineData(2024, 2, 24, "Meak Bochea")]
    [InlineData(2024, 5, 22, "Visak Bochea")]
    [InlineData(2024, 5, 26, "Royal Ploughing Ceremony")]
    [InlineData(2024, 10, 2, "Pchum Ben")]
    [InlineData(2024, 11, 15, "Water Festival")]
    [InlineData(2025, 5, 11, "Visak Bochea")]
    [InlineData(2025, 5, 15, "Royal Ploughing Ceremony")]
    [InlineData(2025, 9, 22, "Pchum Ben")]
    [InlineData(2025, 11, 4, "Water Festival")]
    [InlineData(2025, 11, 6, "Water Festival")]
    public void Observances_FallOnThePublishedHolidayDates(int year, int month, int day, string expected)
    {
        var row = Automatic.CalculateMonth(year, month).Days[day - 1];
        Assert.Equal(expected, row.Observance?.English);
    }

    [Fact]
    public void AsalhaObservances_UseTheSecondAsathInA13MonthYear()
    {
        // 2026 has ខែបឋមាសាឍ and ខែទុតិយាសាឍ; the first Asath full moon is not Asalha Bochea.
        var days = Enumerable.Range(6, 3).SelectMany(month => Automatic.CalculateMonth(2026, month).Days).ToArray();
        var asalha = Assert.Single(days, day => day.Observance?.English == "Asalha Bochea");
        Assert.Equal("ខែទុតិយាសាឍ", asalha.LunarMonth);
        Assert.Contains(days, day => day.LunarMonth == "ខែបឋមាសាឍ" && day.LunarDayNumber == 15);
    }
}
