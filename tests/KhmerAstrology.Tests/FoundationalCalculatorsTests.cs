using KhmerAstrology.Calculation.Charts;
using KhmerAstrology.Calculation.Houses;
using KhmerAstrology.Calculation.Zodiac;

namespace KhmerAstrology.Tests;

public sealed class FoundationalCalculatorsTests
{
    private readonly LongitudeNormalizer _normalizer = new();
    private readonly ZodiacCalculator _zodiac;
    private readonly NakshatraCalculator _nakshatra;

    public FoundationalCalculatorsTests()
    {
        _zodiac = new ZodiacCalculator(_normalizer);
        _nakshatra = new NakshatraCalculator(_normalizer);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(21_600, 0)]
    [InlineData(-1, 21_599)]
    [InlineData(21_601, 1)]
    [InlineData(-21_601, 21_599)]
    public void Normalize_UsesWorkbookFullCircle(int input, double expected)
    {
        Assert.Equal(expected, _normalizer.Normalize(input).ArcMinutes);
    }

    [Fact]
    public void Zodiac_UsesOneBasedSignNumbers()
    {
        Assert.Equal(1, _zodiac.Calculate(0).SignNumber);
        Assert.Equal(2, _zodiac.Calculate(1_800).SignNumber);
        Assert.Equal(12, _zodiac.Calculate(21_599.999).SignNumber);
    }

    [Fact]
    public void Zodiac_ReturnsWorkbookReferenceNamesAndPosition()
    {
        var position = _zodiac.Calculate(5_942.1989849792262);

        Assert.Equal(4, position.SignNumber);
        Assert.Equal("Cancer", position.Sign.NameEn);
        Assert.Equal("កក្កដៈ", position.Sign.NameKm);
        Assert.Equal(9, position.Degree);
        Assert.Equal(2, position.Minute);
        Assert.InRange(position.Second, 11.93, 11.95);
    }

    [Fact]
    public void Nakshatra_UsesEightHundredArcminuteSegments()
    {
        var position = _nakshatra.Calculate(5_942.1989849792262);

        Assert.Equal(8, position.NakshatraNumber);
        Assert.Equal("Pushya", position.Nakshatra.NameEn);
        Assert.Equal("បុស្ស", position.Nakshatra.NameKm);
        Assert.Equal(2, position.Pada);
    }

    [Fact]
    public void Nakshatra_HandlesTheLastPadaAtTheCircleEnd()
    {
        var position = _nakshatra.Calculate(21_599.999);

        Assert.Equal(27, position.NakshatraNumber);
        Assert.Equal(4, position.Pada);
    }

    [Fact]
    public void D1_MatchesWorkbookAscendantSample()
    {
        var calculator = new D1Calculator(_normalizer);

        Assert.Equal(4, calculator.CalculateSign(5_942.1989849792262));
    }

    [Fact]
    public void D9_MatchesWorkbookAscendantSample()
    {
        var calculator = new D9Calculator(_normalizer);

        Assert.Equal(6, calculator.CalculateSign(5_942.1989849792262));
    }

    [Fact]
    public void D3_MatchesWorkbookAscendantSample()
    {
        var calculator = new D3Calculator(_normalizer);

        Assert.Equal(4, calculator.CalculateSign(5_942.1989849792262));
    }

    [Fact]
    public void DivisionalCalculators_NormalizeNegativeLongitudes()
    {
        var d1 = new D1Calculator(_normalizer);
        var d9 = new D9Calculator(_normalizer);
        var d3 = new D3Calculator(_normalizer);

        Assert.Equal(d1.CalculateSign(21_599), d1.CalculateSign(-1));
        Assert.Equal(d9.CalculateSign(21_599), d9.CalculateSign(-1));
        Assert.Equal(d3.CalculateSign(21_599), d3.CalculateSign(-1));
    }

    [Theory]
    [InlineData(4, 4, 1)]
    [InlineData(4, 5, 2)]
    [InlineData(4, 3, 12)]
    [InlineData(12, 1, 2)]
    public void House_UsesWorkbookCompatibleWholeSignFallback(
        int ascendantSign,
        int planetSign,
        int expectedHouse)
    {
        var calculator = new HouseCalculator();

        Assert.Equal(expectedHouse, calculator.CalculateWholeSignHouse(ascendantSign, planetSign));
    }

    [Theory]
    [InlineData(0, 13)]
    [InlineData(13, 1)]
    public void House_RejectsInvalidSignNumbers(int ascendantSign, int planetSign)
    {
        var calculator = new HouseCalculator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.CalculateWholeSignHouse(ascendantSign, planetSign));
    }
}
