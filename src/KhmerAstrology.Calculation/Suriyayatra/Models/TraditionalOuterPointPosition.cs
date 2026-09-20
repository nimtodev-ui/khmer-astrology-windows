using KhmerAstrology.Domain.Enums;

namespace KhmerAstrology.Calculation.Suriyayatra.Models;

public sealed record TraditionalOuterPointPosition(
    CelestialBody Body,
    double LongitudeArcMinutes);
