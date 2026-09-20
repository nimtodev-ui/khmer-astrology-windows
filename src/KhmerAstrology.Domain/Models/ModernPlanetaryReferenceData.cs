namespace KhmerAstrology.Domain.Models;

public sealed record ModernPlanetReference(
    string Body,
    int WorkbookRow,
    double PeriodDays,
    double PhaseOffset);

public sealed record ModernPlanetaryEra(
    int StartYear,
    string TableColumn,
    double StartJulianDay);

public sealed record ModernPlanetaryReferenceData(
    IReadOnlyList<ModernPlanetReference> Bodies,
    IReadOnlyList<ModernPlanetaryEra> Eras,
    IReadOnlyDictionary<string, string[]> Tables);
