using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Domain.Constants;

namespace KhmerAstrology.Calculation.Charts;

public sealed class D1Calculator : IDivisionalChartCalculator
{
    private readonly ILongitudeNormalizer _longitudeNormalizer;

    public D1Calculator(ILongitudeNormalizer longitudeNormalizer)
    {
        _longitudeNormalizer = longitudeNormalizer;
    }

    /// <summary>
    /// Calculates the standard zodiac/D1 sign.
    /// Source: <c>រាសិចក្ក D1-D9-D3!D5:D18</c>.
    /// </summary>
    public int CalculateSign(double longitudeArcMinutes)
    {
        var normalized = _longitudeNormalizer.Normalize(longitudeArcMinutes).ArcMinutes;
        return (int)Math.Floor(normalized / AstrologyConstants.SignArcMinutes) + 1;
    }
}
