using KhmerAstrology.Application.DTOs;
using KhmerAstrology.Application.Interfaces;
using KhmerAstrology.Calculation.Charts;
using KhmerAstrology.Calculation.Interfaces;

namespace KhmerAstrology.Application.Services;

public sealed class FoundationCalculationService : IFoundationCalculationService
{
    private readonly IZodiacCalculator _zodiacCalculator;
    private readonly INakshatraCalculator _nakshatraCalculator;
    private readonly IDivisionalChartCalculator _d1Calculator;
    private readonly IDivisionalChartCalculator _d3Calculator;
    private readonly IDivisionalChartCalculator _d9Calculator;

    public FoundationCalculationService(
        IZodiacCalculator zodiacCalculator,
        INakshatraCalculator nakshatraCalculator,
        D1Calculator d1Calculator,
        D3Calculator d3Calculator,
        D9Calculator d9Calculator)
    {
        _zodiacCalculator = zodiacCalculator;
        _nakshatraCalculator = nakshatraCalculator;
        _d1Calculator = d1Calculator;
        _d3Calculator = d3Calculator;
        _d9Calculator = d9Calculator;
    }

    public FoundationCalculationResult Calculate(double longitudeArcMinutes)
    {
        return new FoundationCalculationResult(
            _zodiacCalculator.Calculate(longitudeArcMinutes),
            _nakshatraCalculator.Calculate(longitudeArcMinutes),
            _d1Calculator.CalculateSign(longitudeArcMinutes),
            _d3Calculator.CalculateSign(longitudeArcMinutes),
            _d9Calculator.CalculateSign(longitudeArcMinutes));
    }
}
