using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Domain.ValueObjects;

namespace KhmerAstrology.Calculation.Zodiac;

public sealed class LongitudeNormalizer : ILongitudeNormalizer
{
    public AstroLongitude Normalize(double longitudeArcMinutes) =>
        new AstroLongitude(longitudeArcMinutes).Normalize();
}
