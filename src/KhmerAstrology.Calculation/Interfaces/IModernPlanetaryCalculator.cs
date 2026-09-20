using KhmerAstrology.Calculation.Suriyayatra.Models;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Interfaces;

public interface IModernPlanetaryCalculator
{
    ModernPlanetaryCalculationResult Calculate(BirthInput input);
}
