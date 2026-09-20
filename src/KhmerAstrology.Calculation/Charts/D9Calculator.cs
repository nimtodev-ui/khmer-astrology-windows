using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Domain.Constants;

namespace KhmerAstrology.Calculation.Charts;

public sealed class D9Calculator : IDivisionalChartCalculator
{
    private readonly ILongitudeNormalizer _longitudeNormalizer;

    public D9Calculator(ILongitudeNormalizer longitudeNormalizer)
    {
        _longitudeNormalizer = longitudeNormalizer;
    }

    /// <summary>
    /// Calculates the Navamsa/D9 sign from the workbook's 200-arcminute segments.
    /// Source: <c>សូរ្យយាត្រ!J17:J30</c> and <c>រាសិចក្ក D1-D9-D3!E5:E18</c>.
    /// </summary>
    public int CalculateSign(double longitudeArcMinutes)
    {
        var normalized = _longitudeNormalizer.Normalize(longitudeArcMinutes).ArcMinutes;
        var navamsaSegment = (int)Math.Floor(normalized / AstrologyConstants.PadaArcMinutes);
        return navamsaSegment % 12 + 1;
    }
}
