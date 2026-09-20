using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Interfaces;

public interface IAscendantCalculator
{
    double CalculateLongitudeArcMinutes(BirthInput input);
}
