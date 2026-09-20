using KhmerAstrology.Domain.Enums;

namespace KhmerAstrology.Calculation.Suriyayatra.Models;

public sealed record ModernPlanetaryPosition(
    CelestialBody Body,
    double LongitudeArcMinutes);

public sealed record ModernPlanetaryCalculationResult(
    double UtcJulianDay,
    IReadOnlyList<ModernPlanetaryPosition> Positions);
