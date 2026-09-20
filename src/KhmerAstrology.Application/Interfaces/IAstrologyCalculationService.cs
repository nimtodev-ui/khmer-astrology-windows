using KhmerAstrology.Application.DTOs;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Application.Interfaces;

public interface IAstrologyCalculationService
{
    AstrologyResult Calculate(BirthInput input);
}
