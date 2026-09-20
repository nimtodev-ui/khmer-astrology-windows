using System.IO.Compression;
using System.Text.Json;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Infrastructure.ReferenceData;

/// <summary>
/// Loads the workbook's encoded automatic-calendar month table. Excel is not
/// required at runtime; the extracted gzip JSON is the deployed reference.
/// </summary>
public sealed class JsonKhmerCalendarMonthReferenceDataSource : IKhmerCalendarMonthReferenceDataSource
{
    private readonly Lazy<IReadOnlyDictionary<(int Year, int Month), KhmerCalendarMonthReference>> _references = new(LoadReferences);

    public KhmerCalendarMonthReference Get(int astronomicalYear, int gregorianMonth)
    {
        if (!_references.Value.TryGetValue((astronomicalYear, gregorianMonth), out var reference))
        {
            throw new ArgumentOutOfRangeException(
                nameof(astronomicalYear),
                $"The workbook automatic-calendar reference does not cover {astronomicalYear}-{gregorianMonth:00}.");
        }

        return reference;
    }

    private static IReadOnlyDictionary<(int Year, int Month), KhmerCalendarMonthReference> LoadReferences()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "khmer-calendar-month-codes.json.gz");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The extracted Khmer calendar month reference data is missing.", path);
        }

        using var file = File.OpenRead(path);
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        var references = JsonSerializer.Deserialize<List<KhmerCalendarMonthReference>>(
            gzip,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (references is null || references.Count == 0)
        {
            throw new InvalidDataException("Khmer calendar month reference data is empty.");
        }

        var invalid = references.Any(reference =>
            reference.AstronomicalYear is < -643 or > 4_459
            || reference.GregorianMonth is < 1 or > 12
            || reference.LunarYearType is < 0 or > 2
            || reference.LunarMonthIndex is < 0 or > 13
            || reference.TithiOffset is < 0 or > 30);
        if (invalid || references.Select(reference => (reference.AstronomicalYear, reference.GregorianMonth)).Distinct().Count() != references.Count)
        {
            throw new InvalidDataException("Khmer calendar month reference data contains invalid or duplicate entries.");
        }

        return references.ToDictionary(
            reference => (reference.AstronomicalYear, reference.GregorianMonth));
    }
}
