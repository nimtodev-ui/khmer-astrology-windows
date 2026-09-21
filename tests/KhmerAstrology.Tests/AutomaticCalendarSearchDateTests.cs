using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Infrastructure.ReferenceData;

namespace KhmerAstrology.Tests;

public sealed class AutomaticCalendarSearchDateTests
{
    private readonly AutomaticCalendarCalculator _calculator = new(new JsonKhmerCalendarYearReferenceDataSource());

    [Fact]
    public void SearchDate_MatchesInit_01January2000()
    {
        // Initialization default test case:
        // Day 1, Month មករា (January = 1), Year 2000
        var result = _calculator.SearchDate(2000, 1, 1);

        // General info
        Assert.Equal(1, result.Day);
        Assert.Equal(1, result.Month);
        Assert.Equal("មករា", result.MonthName);
        Assert.Equal(2000, result.CeOrBceYear);
        Assert.Equal(2000, result.AstronomicalYear);
        Assert.Equal(5100, result.KromSakarajAuto);

        // Section 1: Solar / សុរិយគតិសកល
        Assert.Equal("1 មករា 2000 គ.ស.", result.SolarFullDate);
        Assert.Equal("សៅរ៍", result.SolarWeekday);
        Assert.Equal(1, result.SolarDay);
        Assert.Equal("មករា", result.SolarMonth);
        Assert.Equal("2000 គ.ស.", result.SolarYear);

        // Section 2: Lunar / ចន្ទគតិ
        Assert.Equal("សៅរ៍", result.LunarWeekday);
        Assert.Equal("១០ រោច", result.LunarDay);
        Assert.Equal("បូណ៌ាតិថី", result.TithiName);
        Assert.Equal("ខែមិគសិរ", result.LunarMonth);
        Assert.Equal("ថោះ", result.AnimalYear);

        // Section 3: Other Eras / ឆ្នាំផ្សេងៗ
        Assert.Equal(2543, result.BuddhistYear);
        Assert.Equal(1921, result.MahaSakaraj);
        Assert.Equal(1361, result.ChulaSakaraj);
        Assert.Equal(5100, result.KromSakaraj);
        Assert.Equal(3, result.SesaKalaYoga);
    }

    [Fact]
    public void SearchDate_MatchesUserScreenshot_7August1993()
    {
        // User provided screenshot test case:
        // Day 7, Month សីហា (August = 8), Year 1993
        var result = _calculator.SearchDate(1993, 8, 7);

        // General info
        Assert.Equal(7, result.Day);
        Assert.Equal(8, result.Month);
        Assert.Equal("សីហា", result.MonthName);
        Assert.Equal(1993, result.CeOrBceYear);
        Assert.Equal(1993, result.AstronomicalYear);
        Assert.Equal(5093, result.KromSakarajAuto);

        // Section 1: Solar / សុរិយគតិសកល
        Assert.Equal("7 សីហា 1993 គ.ស.", result.SolarFullDate);
        Assert.Equal("សៅរ៍", result.SolarWeekday);
        Assert.Equal(7, result.SolarDay);
        Assert.Equal("សីហា", result.SolarMonth);
        Assert.Equal("1993 គ.ស.", result.SolarYear);

        // Section 2: Lunar / ចន្ទគតិ
        Assert.Equal("សៅរ៍", result.LunarWeekday);
        Assert.Equal("៥ រោច", result.LunarDay);
        Assert.Equal("បូណ៌ាតិថី", result.TithiName);
        Assert.Equal("ខែទុតិយាសាឍ", result.LunarMonth);
        Assert.Equal("រកា", result.AnimalYear);

        // Section 3: Other Eras / ឆ្នាំផ្សេងៗ
        Assert.Equal(2537, result.BuddhistYear);
        Assert.Equal(1915, result.MahaSakaraj);
        Assert.Equal(1355, result.ChulaSakaraj);
        Assert.Equal(5094, result.KromSakaraj);
        Assert.Equal(4, result.SesaKalaYoga);
    }

    [Fact]
    public void SearchDate_MatchesWorkbookDefault_16September2026()
    {
        // Default workbook sheet 31 values:
        // Day 16, Month កញ្ញា (September = 9), Year 2026
        var result = _calculator.SearchDate(2026, 9, 16);

        Assert.Equal(16, result.Day);
        Assert.Equal(9, result.Month);
        Assert.Equal("កញ្ញា", result.MonthName);
        Assert.Equal(2026, result.CeOrBceYear);
        Assert.Equal(2026, result.AstronomicalYear);
        Assert.Equal(5126, result.KromSakarajAuto);

        // Section 1: Solar / សុរិយគតិសកល
        Assert.Equal("16 កញ្ញា 2026 គ.ស.", result.SolarFullDate);
        Assert.Equal("ពុធ", result.SolarWeekday);
        Assert.Equal(16, result.SolarDay);
        Assert.Equal("កញ្ញា", result.SolarMonth);
        Assert.Equal("2026 គ.ស.", result.SolarYear);

        // Section 2: Lunar / ចន្ទគតិ
        Assert.Equal("ពុធ", result.LunarWeekday);
        Assert.Equal("៥ កើត", result.LunarDay);
        Assert.Equal("បូណ៌ាតិថី", result.TithiName);
        Assert.Equal("ខែភទ្របទ", result.LunarMonth);
        Assert.Equal("មមី", result.AnimalYear);

        // Section 3: Other Eras / ឆ្នាំផ្សេងៗ
        Assert.Equal(2570, result.BuddhistYear);
        Assert.Equal(1948, result.MahaSakaraj);
        Assert.Equal(1388, result.ChulaSakaraj);
        Assert.Equal(5127, result.KromSakaraj);
        Assert.Equal(2, result.SesaKalaYoga);
    }

    [Fact]
    public void SearchDate_SupportsBceYear()
    {
        // BCE -644 (astronomical -643)
        var result = _calculator.SearchDate(-644, 1, 1);

        Assert.Equal(-644, result.CeOrBceYear);
        Assert.Equal(-643, result.AstronomicalYear);
        Assert.Equal(2457, result.KromSakarajAuto);
        Assert.Equal("644 មុន គ.ស.", result.SolarYear);
        Assert.Equal("1 មករា 644 មុន គ.ស.", result.SolarFullDate);
    }

    [Fact]
    public void SearchDate_ThrowsOnInvalidDay()
    {
        // February 2023 has 28 days
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.SearchDate(2023, 2, 29));
        // February 2024 has 29 days
        var leapFeb = _calculator.SearchDate(2024, 2, 29);
        Assert.Equal(29, leapFeb.Day);
    }
}
