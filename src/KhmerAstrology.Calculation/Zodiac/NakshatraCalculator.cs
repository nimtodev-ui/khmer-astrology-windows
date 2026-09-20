using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Domain.Constants;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Domain.ReferenceData;

namespace KhmerAstrology.Calculation.Zodiac;

public sealed class NakshatraCalculator : INakshatraCalculator
{
    private readonly ILongitudeNormalizer _longitudeNormalizer;
    private readonly IReadOnlyList<NakshatraReference> _nakshatras;

    public NakshatraCalculator(ILongitudeNormalizer longitudeNormalizer)
    {
        _longitudeNormalizer = longitudeNormalizer;
        _nakshatras = WorkbookReferenceData.Nakshatras;
    }

    public NakshatraCalculator(
        ILongitudeNormalizer longitudeNormalizer,
        INakshatraReferenceDataSource referenceDataSource)
    {
        _longitudeNormalizer = longitudeNormalizer;
        _nakshatras = referenceDataSource.GetAll();
    }

    /// <summary>
    /// Calculates the 27-Nakshatra index and four-part Pada from an arcminute longitude.
    /// Source: <c>សូរ្យយាត្រ!G17:G30</c> and <c>H17:H30</c>.
    /// </summary>
    public NakshatraPosition Calculate(double longitudeArcMinutes)
    {
        var normalized = _longitudeNormalizer.Normalize(longitudeArcMinutes).ArcMinutes;
        var nakshatraNumber = (int)Math.Floor(normalized / AstrologyConstants.NakshatraArcMinutes) + 1;
        var pada = (int)Math.Floor(
                       (normalized % AstrologyConstants.NakshatraArcMinutes)
                       / AstrologyConstants.PadaArcMinutes)
                   + 1;
        var nakshatra = _nakshatras[nakshatraNumber - 1];

        return new NakshatraPosition(normalized, nakshatraNumber, nakshatra, pada);
    }
}
