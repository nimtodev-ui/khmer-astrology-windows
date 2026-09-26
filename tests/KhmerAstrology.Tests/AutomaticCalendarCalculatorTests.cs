using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Infrastructure.ReferenceData;

namespace KhmerAstrology.Tests;

public sealed class AutomaticCalendarCalculatorTests
{
    private readonly AutomaticCalendarCalculator _calculator = new(new JsonKhmerCalendarYearReferenceDataSource());

    [Fact]
    public void AutomaticCalendar_MatchesWorkbookSeptember2027Sample()
    {
        var result = _calculator.CalculateMonth(2027, 9);

        Assert.Equal(2027, result.AstronomicalYear);
        Assert.Equal(2027, result.CeOrBceYear);
        Assert.Equal(9, result.MonthIndex);
        Assert.Equal("កញ្ញា", result.MonthName);
        Assert.Equal(5127, result.KromSakarajAuto);
        Assert.Equal(30, result.Days.Count);

        // Day 1
        var d1 = result.Days[0];
        Assert.Equal(1, d1.DayNumber);
        Assert.Equal("ពុធ", d1.Weekday);
        Assert.Equal(1, d1.Day);
        Assert.Equal("កញ្ញា", d1.MonthName);
        Assert.Equal(2027, d1.CeYear);
        Assert.Equal(2571, d1.BuddhistYear);
        Assert.Equal(1949, d1.MahaSakaraj);
        Assert.Equal(1389, d1.ChulaSakaraj);
        Assert.Equal(5128, d1.KromSakaraj);
        Assert.Equal("១ កើត", d1.LunarDay);
        Assert.Equal("នន្ទតិថី", d1.TithiName);
        Assert.Equal("ខែភទ្របទ", d1.LunarMonth);
        Assert.Equal("មមែ", d1.AnimalYear);
        Assert.Equal(3, d1.SesaKalaYoga);
        Assert.Equal(41, d1.Yuga);
        Assert.Equal("ប្លវង្គៈ", d1.SamvatsaraName);
        Assert.Equal("មិនទៀងទាត់, អ្វីៗផ្លាស់ប្តូរលឿន", d1.Meaning);

        // Day 15 (Full Moon)
        var d15 = result.Days[14];
        Assert.Equal(15, d15.Day);
        Assert.Equal("ពុធ", d15.Weekday);
        Assert.Equal("១៥ កើត", d15.LunarDay);
        Assert.Equal("ពេញបូណ៌មី", d15.TithiName);

        // Day 16 (1st Waning)
        var d16 = result.Days[15];
        Assert.Equal(16, d16.Day);
        Assert.Equal("ព្រហស្បតិ៍", d16.Weekday);
        Assert.Equal("១ រោច", d16.LunarDay);
        Assert.Equal("នន្ទតិថី", d16.TithiName);

        // Day 29 (14th Waning / Amavasya)
        var d29 = result.Days[28];
        Assert.Equal(29, d29.Day);
        Assert.Equal("ពុធ", d29.Weekday);
        Assert.Equal("១៤ រោច", d29.LunarDay);
        Assert.Equal("អមាវសី", d29.TithiName);

        // Day 30 (15th Waning / Amavasya)
        var d30 = result.Days[29];
        Assert.Equal(30, d30.Day);
        Assert.Equal("ព្រហស្បតិ៍", d30.Weekday);
        Assert.Equal("១៥ រោច", d30.LunarDay);
        Assert.Equal("អមាវសី", d30.TithiName);
    }

    [Fact]
    public void AutomaticCalendar_BceYearCalculation_WorksCorrectly()
    {
        var result = _calculator.CalculateMonth(-500, 1);

        Assert.Equal(-499, result.AstronomicalYear);
        Assert.Equal(-500, result.CeOrBceYear);
        Assert.Equal(1, result.MonthIndex);
        Assert.Equal("មករា", result.MonthName);
        Assert.Equal(2601, result.KromSakarajAuto); // 3101 + (-500)
        Assert.Equal(31, result.Days.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    public void AutomaticCalendar_AllMonthsOf2027_CalculateSuccessfully(int month)
    {
        var result = _calculator.CalculateMonth(2027, month);

        Assert.Equal(2027, result.CeOrBceYear);
        Assert.Equal(month, result.MonthIndex);
        Assert.InRange(result.Days.Count, 28, 31);
        Assert.All(result.Days, day =>
        {
            Assert.False(string.IsNullOrWhiteSpace(day.Weekday));
            Assert.False(string.IsNullOrWhiteSpace(day.LunarDay));
            Assert.False(string.IsNullOrWhiteSpace(day.TithiName));
            Assert.False(string.IsNullOrWhiteSpace(day.LunarMonth));
            Assert.False(string.IsNullOrWhiteSpace(day.AnimalYear));
            Assert.False(string.IsNullOrWhiteSpace(day.SamvatsaraName));
        });
    }
}
