using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Infrastructure.ReferenceData;

public sealed class JsonCelestialBodyReferenceDataSource : ICelestialBodyReferenceDataSource
{
    private readonly Lazy<IReadOnlyList<CelestialBodyReference>> _bodies = new(LoadBodies);

    public IReadOnlyList<CelestialBodyReference> GetAll() => _bodies.Value;

    private static IReadOnlyList<CelestialBodyReference> LoadBodies()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "celestial-bodies.json");
        var file = JsonFile.Read<CelestialBodyFile>(path, "Celestial body");
        if (!string.Equals(file.Status, "workbook-backed", StringComparison.OrdinalIgnoreCase)
            || file.Bodies is null
            || file.Bodies.Count != 13)
        {
            throw new InvalidDataException("Workbook celestial body reference data must contain the 13 catalog rows.");
        }

        var bodies = file.Bodies
            .Select(row => new CelestialBodyReference(
                row.Bodies.Select(ParseBody).ToArray(),
                row.NameKm,
                row.CategoryKm,
                row.Code,
                row.InternationalName,
                row.CycleKm,
                row.MotionKm,
                row.MeaningKm,
                file.Source))
            .ToArray();
        var allBodies = bodies.SelectMany(row => row.Bodies).ToArray();
        if (allBodies.Length == 0 || allBodies.Distinct().Count() != allBodies.Length)
        {
            throw new InvalidDataException("Each celestial body must appear in exactly one catalog row.");
        }
        return bodies;
    }

    private static CelestialBody ParseBody(string name) =>
        Enum.TryParse<CelestialBody>(name, ignoreCase: false, out var body)
            ? body
            : throw new InvalidDataException($"Unknown celestial body '{name}' in celestial body reference data.");

    private sealed class CelestialBodyFile
    {
        public string Status { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public List<CelestialBodyJsonRow>? Bodies { get; init; }
    }

    private sealed class CelestialBodyJsonRow
    {
        public List<string> Bodies { get; init; } = [];
        public string NameKm { get; init; } = string.Empty;
        public string CategoryKm { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string InternationalName { get; init; } = string.Empty;
        public string CycleKm { get; init; } = string.Empty;
        public string MotionKm { get; init; } = string.Empty;
        public string MeaningKm { get; init; } = string.Empty;
    }
}
