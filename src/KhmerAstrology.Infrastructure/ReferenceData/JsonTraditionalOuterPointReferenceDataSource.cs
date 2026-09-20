using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.Infrastructure.ReferenceData;

public sealed class JsonTraditionalOuterPointReferenceDataSource : ITraditionalOuterPointReferenceDataSource
{
    private readonly Lazy<IReadOnlyList<TraditionalOuterPointReference>> _references = new(LoadReferences);

    public IReadOnlyList<TraditionalOuterPointReference> Load() => _references.Value;

    private static IReadOnlyList<TraditionalOuterPointReference> LoadReferences()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "traditional-outer-points.json");
        var references = JsonFile.Read<List<TraditionalOuterPointReference>>(path, "Traditional outer-point");
        if (references.Count != 3
            || references.Any(reference => reference.MeanPeriod <= 0
                || reference.FirstTableBase.Count != 7
                || reference.FirstTableDifference.Count != 7
                || reference.SecondTableBase.Count != 7
                || reference.SecondTableDifference.Count != 7))
        {
            throw new InvalidDataException("Traditional outer-point reference data is incomplete.");
        }
        return references;
    }
}
