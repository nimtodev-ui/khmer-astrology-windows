using KhmerAstrology.Application.DTOs;

namespace KhmerAstrology.Application.Interfaces;

public interface IFoundationCalculationService
{
    FoundationCalculationResult Calculate(double longitudeArcMinutes);
}
