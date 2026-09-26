using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Infrastructure.ReferenceData;

namespace KhmerAstrology.Tests;

public sealed class AutomaticCalendarMonthSummaryTests
{
    private static readonly AutomaticCalendarCalculator Calculator = new(new JsonKhmerCalendarYearReferenceDataSource());

    [Fact]
    public void From_OrdinaryMonth_HasOneValuePerYearField()
    {
        var month = Calculator.CalculateMonth(2027, 9);
        var summary = AutomaticCalendarMonthSummary.From(month);

        Assert.False(summary.BuddhistYear.Changes);
        Assert.False(summary.KromSakaraj.Changes);
        Assert.False(summary.AnimalYear.Changes);
        Assert.Null(summary.YearChangeDay);
        Assert.Equal(month.Days[0].BuddhistYear, summary.BuddhistYear.First);
    }

    [Fact]
    public void From_April_ReportsTheKhmerNewYearChange()
    {
        // Sheet 30 (F6) adds one to ព.ស. from day-of-year 106, so April spans two years.
        var summary = AutomaticCalendarMonthSummary.From(Calculator.CalculateMonth(2027, 4));

        Assert.True(summary.BuddhistYear.Changes);
        Assert.Equal(summary.BuddhistYear.First + 1, summary.BuddhistYear.Last);
        Assert.Equal(summary.BuddhistYear.Last + 2557, summary.KromSakaraj.Last);
        var month = Calculator.CalculateMonth(2027, 4);
        var changeDay = Assert.IsType<int>(summary.YearChangeDay);
        Assert.Equal(summary.BuddhistYear.Last, month.Days[changeDay - 1].BuddhistYear);
        Assert.Equal(summary.BuddhistYear.First, month.Days[changeDay - 2].BuddhistYear);
    }

    [Fact]
    public void From_FindsFullAndNewMoonDaysFromTheTithiColumn()
    {
        var month = Calculator.CalculateMonth(2027, 9);
        var summary = AutomaticCalendarMonthSummary.From(month);

        Assert.NotEmpty(summary.FullMoonDays);
        Assert.All(summary.FullMoonDays, day =>
            Assert.Equal(AutomaticCalendarMonthSummary.FullMoonTithi, month.Days[day - 1].TithiName));
        Assert.All(summary.NewMoonDays, day =>
            Assert.Equal(AutomaticCalendarMonthSummary.NewMoonTithi, month.Days[day - 1].TithiName));
    }

    [Fact]
    public void TithiNames_UseTheWorkbookSpellings()
    {
        // Sheet 30 K6 / sheet 31 B16 formula strings.
        string[] workbookNames =
        [
            "នន្ទតិថី", "ភទ្រតិថី", "ជយតិថី", "រិក្តតិថី", "បូណ៌តិថី",
            AutomaticCalendarMonthSummary.FullMoonTithi, AutomaticCalendarMonthSummary.NewMoonTithi,
        ];
        var produced = Enumerable.Range(1, 12)
            .SelectMany(month => Calculator.CalculateMonth(2026, month).Days)
            .Select(day => day.TithiName)
            .Distinct()
            .ToArray();

        Assert.Equal("អមាវសី", AutomaticCalendarMonthSummary.NewMoonTithi);
        Assert.All(produced, name => Assert.Contains(name, workbookNames));
        Assert.Equal(workbookNames.Length, produced.Length);
    }

    [Theory]
    [InlineData(2026, 1, -1, 2025, 12)]
    [InlineData(2026, 12, 1, 2027, 1)]
    [InlineData(2026, 5, 0, 2026, 5)]
    [InlineData(1, 1, -1, -1, 12)]
    [InlineData(-1, 12, 1, 1, 1)]
    [InlineData(-500, 3, -3, -501, 12)]
    public void StepMonth_CrossesYearsAndSkipsYearZero(int year, int month, int delta, int expectedYear, int expectedMonth)
    {
        Assert.Equal((expectedYear, expectedMonth), AutomaticCalendarMonthSummary.StepMonth(year, month, delta));
    }
}
