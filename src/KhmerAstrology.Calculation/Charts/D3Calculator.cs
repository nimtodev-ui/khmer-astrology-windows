using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Domain.Constants;

namespace KhmerAstrology.Calculation.Charts;

public sealed class D3Calculator : IDivisionalChartCalculator
{
    private readonly ILongitudeNormalizer _longitudeNormalizer;

    public D3Calculator(ILongitudeNormalizer longitudeNormalizer)
    {
        _longitudeNormalizer = longitudeNormalizer;
    }

    /// <summary>
    /// Calculates the Triamsa/D3 sign from the workbook's 600-arcminute segments.
    /// Source: <c>សូរ្យយាត្រ!K17:K30</c> and <c>រាសិចក្ក D1-D9-D3!F5:F18</c>.
    /// </summary>
    public int CalculateSign(double longitudeArcMinutes)
    {
        var normalized = _longitudeNormalizer.Normalize(longitudeArcMinutes).ArcMinutes;
        var signIndexZeroBased = (int)Math.Floor(normalized / AstrologyConstants.SignArcMinutes);
        var triamsaIndexWithinSign = (int)Math.Floor(
            (normalized % AstrologyConstants.SignArcMinutes)
            / AstrologyConstants.TriamsaArcMinutes);

        return (signIndexZeroBased + 4 * triamsaIndexWithinSign) % 12 + 1;
    }
}
