using KhmerAstrology.Domain.ValueObjects;

namespace KhmerAstrology.Calculation.Interfaces;

public interface ILongitudeNormalizer
{
    AstroLongitude Normalize(double longitudeArcMinutes);
}
