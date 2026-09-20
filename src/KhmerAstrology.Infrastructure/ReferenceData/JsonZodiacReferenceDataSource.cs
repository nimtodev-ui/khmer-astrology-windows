using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Infrastructure.ReferenceData;

public sealed class JsonZodiacReferenceDataSource : IZodiacReferenceDataSource
{
    private readonly Lazy<IReadOnlyList<ZodiacSignReference>> _signs = new(LoadSigns);

    public IReadOnlyList<ZodiacSignReference> GetAll() => _signs.Value;

    private static IReadOnlyList<ZodiacSignReference> LoadSigns()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "zodiac-signs.json");
        var signs = JsonFile.ReadArray(path, "Zodiac sign", item => new ZodiacSignReference(
            item.GetProperty("number").GetInt32(),
            item.GetProperty("nameEn").GetString() ?? string.Empty,
            item.GetProperty("nameKm").GetString() ?? string.Empty,
            item.GetProperty("symbol").GetString() ?? string.Empty));
        var signNumbers = signs.Select(sign => sign.Number).ToArray();
        if (signs.Count != 12 || signNumbers.Min() != 1 || signNumbers.Max() != 12 || signNumbers.Distinct().Count() != 12)
        {
            throw new InvalidDataException("Zodiac reference data must contain signs 1 through 12.");
        }
        return signs;
    }
}
