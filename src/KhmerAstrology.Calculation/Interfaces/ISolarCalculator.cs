using KhmerAstrology.Calculation.Suriyayatra.Models;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Interfaces;

public interface ISolarCalculator
{
    SolarCalculationResult Calculate(KhmerCalendarResult calendar);
}
