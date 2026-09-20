using KhmerAstrology.Domain.Enums;

namespace KhmerAstrology.Domain.Models;

public sealed class PlanetPosition
{
    public CelestialBody Body { get; init; }

    public double LongitudeArcMinutes { get; init; }

    public int SignNumber { get; init; }

    public string SignNameEn { get; init; } = string.Empty;

    public string SignNameKm { get; init; } = string.Empty;

    public int Degree { get; init; }

    public int Minute { get; init; }

    public double Second { get; init; }

    public int NakshatraNumber { get; init; }

    public string NakshatraName { get; init; } = string.Empty;

    public int Pada { get; init; }

    public int House { get; init; }
}
