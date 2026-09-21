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

    private static readonly IReadOnlyList<object[]> Sheet2Rows =
    [
        // Body, Longitude, SignIndex, Deg, Min, Sec, Nakshatra, Pada, TypeIndex (0-based: 0=ទលិទ្ទោ, 1=មហទ្ធនោ, 2=ចោរោ, 3=ភូមិបាលោ, 4=វេសិយោ, 5=ទេវី, 6=ពេជ្ឈឃាតោ, 7=រាជា, 8=សមណោ), D9SignIndex, D3SignIndex
        new object[] { "Lagna (Ascendant)", 2_961.5007818897216D, 1, 19, 21, 30.00D, 4, 3, 3, 2, 5 },
        new object[] { "Sun", 11_483.728512255722D, 6, 11, 23, 43.71D, 15, 2, 5, 9, 10 },
        new object[] { "Moon", 8_891.5565868411722D, 4, 28, 11, 33.40D, 12, 1, 2, 8, 0 },
        new object[] { "Mars", 5_602.4635633703701D, 3, 3, 22, 27.81D, 8, 1, 7, 4, 3 },
        new object[] { "Mercury", 12_498.217087363319D, 6, 28, 18, 13.03D, 16, 3, 6, 2, 2 },
        new object[] { "Jupiter", 3_391.1107725866577D, 1, 26, 31, 6.65D, 5, 1, 4, 4, 9 },
        new object[] { "Venus", 13_724.517315768433D, 7, 18, 44, 31.04D, 18, 1, 8, 8, 11 },
        new object[] { "Saturn", 19_126.331729715519D, 10, 18, 46, 19.90D, 24, 4, 5, 11, 2 },
        new object[] { "Rahu", 20_536.560994679141D, 11, 12, 16, 33.66D, 26, 3, 7, 6, 3 },
        new object[] { "Ketu Veda", 9_736.560994679141D, 5, 12, 16, 33.66D, 13, 1, 3, 0, 9 },
        new object[] { "Ketu Divya", 13_780.579209636036D, 7, 19, 40, 34.75D, 18, 1, 8, 8, 11 },
        new object[] { "Uranus", 1_909.9003595359013D, 1, 1, 49, 54.02D, 3, 2, 2, 9, 1 },
        new object[] { "Neptune", 20_001.957351659843D, 11, 3, 21, 57.44D, 26, 1, 7, 4, 11 },
        new object[] { "Pluto", 16_530.243119455332D, 9, 5, 30, 14.59D, 21, 3, 2, 10, 9 }
    ];

    public static IEnumerable<object[]> Sheet2RowsData => Sheet2Rows;

    [Theory]
    [MemberData(nameof(Sheet2RowsData))]
    public void Sheet2_MasterRow17To30_CalculationsMatchExactly(
        string body,
        double totalMinutes,
        int expectedSignIndex,
        int expectedDeg,
        int expectedMin,
        double expectedSec,
        int expectedNakshatra,
        int expectedPada,
        int expectedTypeIndex,
        int expectedD9SignIndex,
        int expectedD3SignIndex)
    {
        var modMinutes = ((totalMinutes % 21600D) + 21600D) % 21600D;
        var signIndex = (int)(modMinutes / 1800D);
        var deg = (int)((modMinutes % 1800D) / 60D);
        var min = (int)(modMinutes % 60D);
        var secDecimals = body.StartsWith("Lagna", StringComparison.Ordinal) ? 0 : 2;
        var secVal = Math.Round((modMinutes % 1D) * 60D, secDecimals);

        var nakshatraIndex = (int)(modMinutes / 800D) + 1;
        var pada = (int)((modMinutes % 800D) / 200D) + 1;
        var typeIndex = (nakshatraIndex - 1) % 9;

        var d9SignIndex = ((int)(modMinutes / 200D)) % 12;
        var drekkanaInSign = (int)((modMinutes % 1800D) / 600D) + 1;
        var d3SignIndex = (signIndex + 4 * (drekkanaInSign - 1)) % 12;

        Assert.Equal(expectedSignIndex, signIndex);
        Assert.Equal(expectedDeg, deg);
        Assert.Equal(expectedMin, min);
        Assert.InRange(secVal, expectedSec - 0.01D, expectedSec + 0.01D);
        Assert.Equal(expectedNakshatra, nakshatraIndex);
        Assert.Equal(expectedPada, pada);
        Assert.Equal(expectedTypeIndex, typeIndex);
        Assert.Equal(expectedD9SignIndex, d9SignIndex);
        Assert.Equal(expectedD3SignIndex, d3SignIndex);
        Assert.False(string.IsNullOrWhiteSpace(body));
    }
}
