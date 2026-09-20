using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Infrastructure.ReferenceData;

public sealed class JsonNakshatraReferenceDataSource : INakshatraReferenceDataSource
{
    private readonly Lazy<IReadOnlyList<NakshatraReference>> _nakshatras = new(LoadNakshatras);

    public IReadOnlyList<NakshatraReference> GetAll() => _nakshatras.Value;

    private static IReadOnlyList<NakshatraReference> LoadNakshatras()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "nakshatras.json");
        var nakshatras = JsonFile.ReadArray(path, "Nakshatra", item => new NakshatraReference(
            item.GetProperty("number").GetInt32(),
            item.GetProperty("nameEn").GetString() ?? string.Empty,
            item.GetProperty("nameKm").GetString() ?? string.Empty,
            item.GetProperty("startArcMinute").GetDouble()));
        var nakshatraNumbers = nakshatras.Select(item => item.Number).ToArray();
        if (nakshatras.Count != 27 || nakshatraNumbers.Min() != 1 || nakshatraNumbers.Max() != 27 || nakshatraNumbers.Distinct().Count() != 27)
        {
            throw new InvalidDataException("Nakshatra reference data must contain entries 1 through 27.");
        }
        return nakshatras;
    }
}
