using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Domain.Constants;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Domain.ReferenceData;

namespace KhmerAstrology.Calculation.Zodiac;

public sealed class ZodiacCalculator : IZodiacCalculator
{
    private readonly ILongitudeNormalizer _longitudeNormalizer;
    private readonly IReadOnlyList<ZodiacSignReference> _signs;

    public ZodiacCalculator(ILongitudeNormalizer longitudeNormalizer)
    {
        _longitudeNormalizer = longitudeNormalizer;
        _signs = WorkbookReferenceData.ZodiacSigns;
    }

    public ZodiacCalculator(
        ILongitudeNormalizer longitudeNormalizer,
        IZodiacReferenceDataSource referenceDataSource)
    {
        _longitudeNormalizer = longitudeNormalizer;
        _signs = referenceDataSource.GetAll();
    }

    /// <summary>
    /// Calculates the D1 zodiac sign and the degree/minute/second position inside it.
    /// Source: <c>សូរ្យយាត្រ!C17:C30</c>, <c>D17:F30</c>.
    /// </summary>
    public ZodiacPosition Calculate(double longitudeArcMinutes)
    {
        var normalized = _longitudeNormalizer.Normalize(longitudeArcMinutes).ArcMinutes;
        var signNumber = (int)Math.Floor(normalized / AstrologyConstants.SignArcMinutes) + 1;
        var sign = _signs[signNumber - 1];
        var withinSign = normalized % AstrologyConstants.SignArcMinutes;
        var degree = (int)Math.Floor(withinSign / 60D);
        var minute = (int)Math.Floor(withinSign % 60D);
        var second = Math.Round((withinSign % 1D) * 60D, 2, MidpointRounding.AwayFromZero);

        return new ZodiacPosition(normalized, signNumber, sign, degree, minute, second);
    }
}
