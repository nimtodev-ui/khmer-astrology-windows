using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Interfaces;

public interface INakshatraCalculator
{
    NakshatraPosition Calculate(double longitudeArcMinutes);
}
