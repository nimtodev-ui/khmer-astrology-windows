using KhmerAstrology.Domain.Enums;

namespace KhmerAstrology.Domain.Models;

/// <summary>
/// One row of the workbook planet catalog sheet `សរុបតារាគ្រោះ`.
/// A row can cover several bodies, e.g. Mrityu and the modern Uranus chain.
/// </summary>
public sealed record CelestialBodyReference(
    IReadOnlyList<CelestialBody> Bodies,
    string NameKm,
    string CategoryKm,
    string Code,
    string InternationalName,
    string CycleKm,
    string MotionKm,
    string MeaningKm,
    string Source);
