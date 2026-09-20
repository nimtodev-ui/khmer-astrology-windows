using System.IO.Compression;
using System.Text.Json;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Infrastructure.ReferenceData;

/// <summary>
/// Loads the workbook-extracted modern planetary coefficient tables.
/// The workbook itself is never opened by the application.
/// </summary>
public sealed class JsonModernPlanetaryReferenceDataSource : IModernPlanetaryReferenceDataSource
{
    private readonly Lazy<ModernPlanetaryReferenceData> _data;

    public JsonModernPlanetaryReferenceDataSource()
    {
        _data = new Lazy<ModernPlanetaryReferenceData>(LoadFromCompressedJson);
    }

    public ModernPlanetaryReferenceData Load() => _data.Value;

    private static ModernPlanetaryReferenceData LoadFromCompressedJson()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "modern-planetary-coefficients.json.gz");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "The extracted modern planetary reference data is missing.",
                path);
        }

        using var file = File.OpenRead(path);
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        var data = JsonSerializer.Deserialize<ModernPlanetaryReferenceData>(
            gzip,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return data ?? throw new InvalidDataException("Modern planetary reference data is empty.");
    }
}
