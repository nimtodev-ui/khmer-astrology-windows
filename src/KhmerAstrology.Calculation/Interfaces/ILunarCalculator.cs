using KhmerAstrology.Calculation.Suriyayatra.Models;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Interfaces;

public interface ILunarCalculator
{
    LunarCalculationResult Calculate(KhmerCalendarResult calendar, SolarCalculationResult sun);
}
