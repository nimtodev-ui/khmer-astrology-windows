using KhmerAstrology.Domain.Enums;

namespace KhmerAstrology.Domain.Models;

public sealed class AstrologyChart
{
    public ChartType ChartType { get; init; }

    public IReadOnlyList<AstrologyChartHouse> Houses { get; init; } = [];
}

public sealed class AstrologyChartHouse
{
    public int HouseNumber { get; init; }

    public int SignNumber { get; init; }

    public string SignNameEn { get; init; } = string.Empty;

    public string SignNameKm { get; init; } = string.Empty;

    public IReadOnlyList<AstrologyChartPlacement> Placements { get; init; } = [];
}

public sealed class AstrologyChartPlacement
{
    public CelestialBody Body { get; init; }

    public string DisplayName { get; init; } = string.Empty;
}
