using KhmerAstrology.Domain.Models;
using KhmerAstrology.Calculation.Suriyayatra.Models;

namespace KhmerAstrology.Application.DTOs;

public sealed class AstrologyResult
{
    public BirthInput BirthInput { get; init; } = null!;

    public KhmerCalendarResult KhmerCalendar { get; init; } = null!;

    public SolarCalculationResult TraditionalSun { get; init; } = null!;

    public LunarCalculationResult TraditionalMoon { get; init; } = null!;

    public PlanetPosition Ascendant { get; init; } = null!;

    public IReadOnlyList<PlanetPosition> Planets { get; init; } = [];

    public IReadOnlyList<PlanetPosition> AdditionalPoints { get; init; } = [];

    public AstrologyChart D1 { get; init; } = null!;

    public AstrologyChart D3 { get; init; } = null!;

    public AstrologyChart D9 { get; init; } = null!;

    public IReadOnlyList<InterpretationResult> Interpretations { get; init; } = [];

    public string CalculationStage { get; init; } = string.Empty;
}
