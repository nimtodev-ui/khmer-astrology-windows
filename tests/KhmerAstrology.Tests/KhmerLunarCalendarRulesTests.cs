using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Infrastructure.ReferenceData;

namespace KhmerAstrology.Tests;

public sealed class KhmerLunarCalendarRulesTests
{
    private static readonly AutomaticCalendarCalculator Automatic = new(new JsonKhmerCalendarYearReferenceDataSource());

    [Theory]
    [InlineData(1381, "ឯកស័ក")]
    [InlineData(1382, "ទោស័ក")]
    [InlineData(1383, "ត្រីស័ក")]
    [InlineData(1384, "ចត្វាស័ក")]
    [InlineData(1385, "បញ្ចស័ក")]
    [InlineData(1386, "ឆស័ក")]       // 2024–25: ឆ្នាំរោង ឆស័ក
    [InlineData(1387, "សប្តស័ក")]    // 2025–26: ឆ្នាំម្សាញ់ សប្តស័ក
    [InlineData(1388, "អដ្ឋស័ក")]    // 2026–27: ឆ្នាំមមី អដ្ឋស័ក
    [InlineData(1389, "នព្វស័ក")]
    [InlineData(1390, "សំរឹទ្ធិស័ក")]
    public void Sak_FollowsTheLastDigitOfChulaSakaraj(int chulaSakaraj, string expected)
    {
        Assert.Equal(expected, KhmerLunarCalendarRules.GetSakName(chulaSakaraj, khmer: true));
    }

    [Fact]
    public void Sak_ChangesWithTheWorkbookChulaSakarajAtKhmerNewYear()
    {
        var beforeNewYear = Automatic.SearchDate(2025, 1, 15);
        var afterNewYear = Automatic.SearchDate(2025, 5, 15);

        Assert.Equal("ឆស័ក", KhmerLunarCalendarRules.GetSakName(beforeNewYear.ChulaSakaraj, khmer: true));
        Assert.Equal("សប្តស័ក", KhmerLunarCalendarRules.GetSakName(afterNewYear.ChulaSakaraj, khmer: true));
    }

    [Theory]
    [InlineData(8, 30, true)]
    [InlineData(15, 30, true)]
    [InlineData(23, 30, true)]
    [InlineData(30, 30, true)]   // 15 រោច ends a 30-day month
    [InlineData(29, 29, true)]   // 14 រោច ends a 29-day month
    [InlineData(29, 30, false)]  // 14 រោច is not the last day of a 30-day month
    [InlineData(1, 30, false)]
    [InlineData(14, 30, false)]
    public void HolyDay_IsDay8And15OfEachFortnightOrTheMonthsLastDay(int lunarDay, int monthLength, bool expected)
    {
        Assert.Equal(expected, KhmerLunarCalendarRules.IsHolyDay(lunarDay, monthLength));
    }

    [Theory]
    [InlineData(2024, 5, 22)]   // Visak Bochea, 15 កើត ពិសាខ
    [InlineData(2024, 10, 2)]   // Pchum Ben, 15 រោច ភទ្របទ (30-day month)
    [InlineData(2024, 10, 17)]  // End of Lent, 15 កើត អស្សុជ
    public void FestivalFullAndLastDays_AreHolyDays(int year, int month, int day)
    {
        Assert.True(Automatic.SearchDate(year, month, day).IsHolyDay);
        Assert.True(Automatic.GetLunarState(year, month, day).IsHolyDay);
    }

    [Fact]
    public void EveryCompleteLunarMonthOf2024_HasFourHolyDays()
    {
        var days = new List<(DateOnly Date, KhmerLunarState State)>();
        for (var date = new DateOnly(2024, 1, 1); date.Year == 2024; date = date.AddDays(1))
        {
            days.Add((date, Automatic.GetLunarState(date.Year, date.Month, date.Day)));
        }

        // Group consecutive days into lunar months (a new month starts on lunar day 1).
        var months = new List<List<KhmerLunarState>>();
        foreach (var (_, state) in days)
        {
            if (state.LunarDay == 1 || months.Count == 0)
            {
                months.Add([]);
            }
            months[^1].Add(state);
        }

        var complete = months.Where(month => month[0].LunarDay == 1 && month.Count == month[0].MonthLength).ToArray();
        Assert.True(complete.Length >= 11);
        Assert.All(complete, month => Assert.Equal(4, month.Count(state => state.IsHolyDay)));
        Assert.Contains(complete, month => month[0].MonthLength == 29);
        Assert.Contains(complete, month => month[0].MonthLength == 30);
    }
}
