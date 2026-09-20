using KhmerAstrology.Calculation.Charts;
using KhmerAstrology.Calculation.Zodiac;

namespace KhmerAstrology.Tests;

/// <summary>
/// First Excel comparison fixtures. Values come from the cached results in
/// <c>សូរ្យយាត្រ!B17:B22</c> and derived fields in <c>A17:K22</c>.
/// These fixtures intentionally cover only the foundational formulas; the
/// planetary source cells are not being reimplemented in this phase.
/// </summary>
public sealed class WorkbookComparisonTests
{
    private static readonly IReadOnlyList<object[]> WorkbookRows =
    [
        new object[] { "Ascendant B17", 5_942.1989849792262D, 4, 8, 2, 6, 4 },
        new object[] { "Sun B18", 7_418.1025339840035D, 5, 10, 2, 2, 5 },
        new object[] { "Moon B19", 13_324.781560153069D, 8, 17, 3, 7, 12 },
        new object[] { "Mars B20", 4_321.3374349146325D, 3, 6, 2, 10, 7 },
        new object[] { "Mercury B21", 6_994.2598918044505D, 4, 9, 3, 11, 12 },
        new object[] { "Jupiter B22", 6_425.7319357357292D, 4, 9, 1, 9, 8 }
    ];

    public static IEnumerable<object[]> WorkbookRowsData => WorkbookRows;

    [Theory]
    [MemberData(nameof(WorkbookRowsData))]
    public void FoundationalOutputs_MatchWorkbookDerivedColumns(
        string sourceCell,
        double longitudeArcMinutes,
        int expectedD1,
        int expectedNakshatra,
        int expectedPada,
        int expectedD9,
        int expectedD3)
    {
        var normalizer = new LongitudeNormalizer();
        var zodiac = new ZodiacCalculator(normalizer);
        var nakshatra = new NakshatraCalculator(normalizer);
        var d1 = new D1Calculator(normalizer);
        var d9 = new D9Calculator(normalizer);
        var d3 = new D3Calculator(normalizer);

        var zodiacPosition = zodiac.Calculate(longitudeArcMinutes);
        var nakshatraPosition = nakshatra.Calculate(longitudeArcMinutes);

        Assert.Equal(expectedD1, zodiacPosition.SignNumber);
        Assert.Equal(expectedD1, d1.CalculateSign(longitudeArcMinutes));
        Assert.Equal(expectedNakshatra, nakshatraPosition.NakshatraNumber);
        Assert.Equal(expectedPada, nakshatraPosition.Pada);
        Assert.Equal(expectedD9, d9.CalculateSign(longitudeArcMinutes));
        Assert.Equal(expectedD3, d3.CalculateSign(longitudeArcMinutes));
        Assert.False(string.IsNullOrWhiteSpace(sourceCell));
    }
}
