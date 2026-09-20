using KhmerAstrology.Calculation.Suriyayatra.Models;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Calculation.Interfaces;

public interface ITraditionalOuterPointCalculator
{
    IReadOnlyList<TraditionalOuterPointPosition> Calculate(
        KhmerCalendarResult calendar,
        SolarCalculationResult solar);
}
