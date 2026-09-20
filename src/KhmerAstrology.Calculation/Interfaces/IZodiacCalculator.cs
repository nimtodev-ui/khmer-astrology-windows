using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Interfaces;

public interface IZodiacCalculator
{
    ZodiacPosition Calculate(double longitudeArcMinutes);
}
