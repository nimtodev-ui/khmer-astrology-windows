using System.Text.Json;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Infrastructure.ReferenceData;

public sealed class JsonLocationReferenceDataSource : ILocationReferenceDataSource
{
    private readonly Lazy<IReadOnlyList<AstrologyLocation>> _locations = new(LoadLocations);

    public IReadOnlyList<AstrologyLocation> GetAll() => _locations.Value;

    private static IReadOnlyList<AstrologyLocation> LoadLocations()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "cambodia-locations.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Cambodian location reference data was not deployed.", path);
        }

        var json = File.ReadAllText(path);
        var locations = JsonSerializer.Deserialize<List<AstrologyLocation>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });
        if (locations is null || locations.Count == 0)
        {
            throw new InvalidDataException("Cambodian location reference data is empty.");
        }

        return locations;
    }
}
